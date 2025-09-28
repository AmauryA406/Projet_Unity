// ========== RENARD.CS CORRIGÉ AVEC ENUM ==========
using UnityEngine;
using System.Collections;

namespace EcosystemSimulation
{
    // ENUM nécessaire pour les états du renard
    public enum EtatRenard
    {
        Patrouille,
        Chasse,
        Attaque
    }

    public class Renard : Animal
    {
        [Header("Stats Renard")]
        [SerializeField] private float rayonDetectionProie = 10f;
        [SerializeField] private float tempsAttaque = 3f;
        [SerializeField] private float seuilReproduction = 100f; // Réduit pour le nouveau système
        [SerializeField] private float cooldownReproduction = 20f;
        [SerializeField] private LayerMask layerProie;
        [SerializeField] private GameObject prefabRenard;

        private EtatRenard etatActuel = EtatRenard.Patrouille;
        private Transform cibleActuelle;
        private bool enAttaque = false;
        private bool peutSeReproduire = true;
        private Rigidbody rb;

        protected override void Start()
        {
            // Configuration spécifique aux renards
            energieMax = 150f;
            vitesseBase = 4f;
            perteEnergieParSeconde = 0.5f;
            tempsJusquMaturite = 8f; // Renards maturent plus lentement

            rb = GetComponent<Rigidbody>();
            base.Start();
        }

        protected override void Update()
        {
            base.Update();

            if (!enAttaque)
            {
                GererComportement();
                TenterReproduction();
            }
        }

        protected override void ChangerApparenceMaturite()
        {
            base.ChangerApparenceMaturite();

            // Renards adultes deviennent plus gros et plus sombres
            transform.localScale = transform.localScale * 1.15f;

            if (TryGetComponent<Renderer>(out Renderer renderer))
            {
                Color couleur = renderer.material.color;
                renderer.material.color = new Color(couleur.r * 0.8f, couleur.g * 0.8f, couleur.b * 0.8f, couleur.a);
            }

            Debug.Log($"Renard {gameObject.name} a atteint sa maturité de prédateur !");
        }

        private void TenterReproduction()
        {
            if (energieActuelle >= seuilReproduction &&
                EstAdulte() && // Vérifier la maturité
                peutSeReproduire &&
                prefabRenard != null)
            {
                if (EcosystemManager.PeutSeReproduire("Renard", 4))
                {
                    StartCoroutine(SeReproduire());
                }
            }
        }

        private IEnumerator SeReproduire()
        {
            peutSeReproduire = false;

            Debug.Log($"Renard {gameObject.name} (âge: {ObtenirAge():F1}s) se reproduit !");

            Vector3 positionBebe = transform.position + EcosystemManager.GenererPositionAleatoire(2f, 5f);
            GameObject nouveauRenard = EcosystemManager.CreerAnimal(prefabRenard, positionBebe);

            if (nouveauRenard != null)
            {
                nouveauRenard.tag = "Renard";
                nouveauRenard.name = $"Renard_Enfant_{Random.Range(1000, 9999)}";
            }

            energieActuelle -= 50f;

            yield return new WaitForSeconds(cooldownReproduction);
            peutSeReproduire = true;
        }

        private void GererComportement()
        {
            switch (etatActuel)
            {
                case EtatRenard.Patrouille:
                    ChercherProie();
                    MouvementAleatoire();
                    break;

                case EtatRenard.Chasse:
                    PoursuivreProie();
                    break;
            }
        }

        private void ChercherProie()
        {
            // Les jeunes renards ont une détection réduite
            float rayonEffectif = EstAdulte() ? rayonDetectionProie : rayonDetectionProie * 0.7f;

            Collider[] proies = Physics.OverlapSphere(transform.position, rayonEffectif, layerProie);

            if (proies.Length > 0)
            {
                Transform procheCible = null;
                float distanceMin = float.MaxValue;

                foreach (Collider proie in proies)
                {
                    float distance = Vector3.Distance(transform.position, proie.transform.position);
                    if (distance < distanceMin)
                    {
                        distanceMin = distance;
                        procheCible = proie.transform;
                    }
                }

                cibleActuelle = procheCible;
                etatActuel = EtatRenard.Chasse;
            }
        }

        private void PoursuivreProie()
        {
            if (cibleActuelle == null)
            {
                etatActuel = EtatRenard.Patrouille;
                return;
            }

            Vector3 direction = (cibleActuelle.position - transform.position).normalized;

            // Les adultes chassent plus efficacement
            float facteurVitesse = EstAdulte() ? 1f : 0.6f;
            rb.linearVelocity = new Vector3(direction.x * vitesseBase * facteurVitesse, rb.linearVelocity.y, direction.z * vitesseBase * facteurVitesse);

            // Attaquer si assez proche
            if (Vector3.Distance(transform.position, cibleActuelle.position) < 2f && !enAttaque)
            {
                StartCoroutine(Attaquer());
            }
        }

        private IEnumerator Attaquer()
        {
            enAttaque = true;
            etatActuel = EtatRenard.Attaque;

            yield return new WaitForSeconds(tempsAttaque);

            if (cibleActuelle != null && Vector3.Distance(transform.position, cibleActuelle.position) < 3f)
            {
                // Réussir l'attaque
                Destroy(cibleActuelle.gameObject);
                GagnerEnergie(40f);
                Debug.Log($"Renard {gameObject.name} a attrapé sa proie !");
            }

            enAttaque = false;
            cibleActuelle = null;
            etatActuel = EtatRenard.Patrouille;
        }

        protected override void Deplacer()
        {
            // La logique de mouvement est déjà dans GererComportement
        }

        protected override void MouvementAleatoire()
        {
            if (rb == null) return;

            Vector3 directionAleatoire = new Vector3(
                Random.Range(-1f, 1f),
                0f,
                Random.Range(-1f, 1f)
            ).normalized;

            // Les jeunes renards bougent plus lentement
            float facteurVitesse = EstAdulte() ? 0.4f : 0.2f;

            rb.linearVelocity = new Vector3(
                directionAleatoire.x * vitesseBase * facteurVitesse,
                rb.linearVelocity.y,
                directionAleatoire.z * vitesseBase * facteurVitesse
            );
        }

        // Gizmos pour visualiser les rayons de détection
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            float rayonEffectif = EstAdulte() ? rayonDetectionProie : rayonDetectionProie * 0.7f;
            Gizmos.DrawWireSphere(transform.position, rayonEffectif);
        }
    }
}