using UnityEngine;
using System.Collections;

namespace EcosystemSimulation
{
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
        [SerializeField] private float seuilReproduction = 100f;
        [SerializeField] private float cooldownReproduction = 20f;
        [SerializeField] private LayerMask layerProie;
        [SerializeField] private GameObject prefabRenard;

        [Header("Patrouille")]
        [SerializeField] private float tempsChangementDirection = 4f; // Plus long que les lapins
        [SerializeField] private float vitessePatrouille = 0.3f; // Plus lent en patrouille
        [SerializeField] private float tempsArretAleatoire = 2f; // Arrêts plus longs
        [SerializeField] private float distancePatrouille = 8f; // Distance de patrouille

        private EtatRenard etatActuel = EtatRenard.Patrouille;
        private Transform cibleActuelle;
        private bool enAttaque = false;
        private bool peutSeReproduire = true;
        private Rigidbody rb;

        // Variables pour la patrouille
        private Vector3 directionPatrouille;
        private float tempsProchainChangement;
        private bool enArret = false;
        private float tempsFinArret;
        private Vector3 pointPatrouilleOrigine;

        protected override void Start()
        {
            // Configuration spécifique aux renards
            energieMax = 150f;
            vitesseBase = 4f;
            perteEnergieParSeconde = 0.5f;
            tempsJusquMaturite = 8f;

            rb = GetComponent<Rigidbody>();
            base.Start();

            // Initialiser la patrouille
            pointPatrouilleOrigine = transform.position;
            InitialiserPatrouille();
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

        private void InitialiserPatrouille()
        {
            // Générer une direction aléatoire pour la patrouille
            directionPatrouille = new Vector3(
                Random.Range(-1f, 1f),
                0f,
                Random.Range(-1f, 1f)
            ).normalized;

            // Définir le prochain changement de direction
            tempsProchainChangement = Time.time + Random.Range(
                tempsChangementDirection * 0.7f,
                tempsChangementDirection * 1.3f
            );
        }

        protected override void ChangerApparenceMaturite()
        {
            base.ChangerApparenceMaturite();
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
                EstAdulte() &&
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
                    if (cibleActuelle == null) // Seulement patrouiller s'il n'y a pas de proie
                    {
                        MouvementAleatoire();
                    }
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
                Debug.Log($"Renard {gameObject.name} a détecté une proie : {cibleActuelle.name}");
            }
        }

        private void PoursuivreProie()
        {
            if (cibleActuelle == null)
            {
                etatActuel = EtatRenard.Patrouille;
                Debug.Log($"Renard {gameObject.name} a perdu sa proie, retour en patrouille");
                return;
            }

            Vector3 direction = (cibleActuelle.position - transform.position).normalized;

            // Les adultes chassent plus efficacement
            float facteurVitesse = EstAdulte() ? 1f : 0.6f;
            rb.linearVelocity = new Vector3(
                direction.x * vitesseBase * facteurVitesse,
                rb.linearVelocity.y,
                direction.z * vitesseBase * facteurVitesse
            );

            // Attaquer si assez proche
            if (Vector3.Distance(transform.position, cibleActuelle.position) < 2f && !enAttaque)
            {
                StartCoroutine(Attaquer());
            }

            // Abandonner la poursuite si trop loin
            if (Vector3.Distance(transform.position, cibleActuelle.position) > rayonDetectionProie * 1.5f)
            {
                cibleActuelle = null;
                etatActuel = EtatRenard.Patrouille;
                Debug.Log($"Renard {gameObject.name} abandonne la poursuite");
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
            else
            {
                Debug.Log($"Renard {gameObject.name} a raté son attaque");
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

            // Vérifier si on est en arrêt
            if (enArret)
            {
                if (Time.time >= tempsFinArret)
                {
                    enArret = false;
                    InitialiserPatrouille(); // Nouvelle direction après l'arrêt
                }
                else
                {
                    // Rester immobile pendant l'arrêt (comportement de chasseur à l'affût)
                    rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
                    return;
                }
            }

            // Vérifier s'il faut changer de direction
            if (Time.time >= tempsProchainChangement)
            {
                // Chance d'avoir un arrêt aléatoire (renards s'arrêtent pour écouter/observer)
                if (Random.Range(0f, 1f) < 0.4f) // 40% de chance d'arrêt
                {
                    enArret = true;
                    tempsFinArret = Time.time + Random.Range(1f, tempsArretAleatoire);
                    rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
                    return;
                }
                else
                {
                    // Nouvelle direction de patrouille
                    GenerarNouvelleDirectionPatrouille();
                }
            }

            // Appliquer le mouvement de patrouille
            float facteurVitesse = EstAdulte() ? vitessePatrouille : vitessePatrouille * 0.7f;

            rb.linearVelocity = new Vector3(
                directionPatrouille.x * vitesseBase * facteurVitesse,
                rb.linearVelocity.y,
                directionPatrouille.z * vitesseBase * facteurVitesse
            );

            // Debug pour visualiser la patrouille
            Debug.DrawRay(transform.position, directionPatrouille * 4f, Color.red, 0.1f);
        }

        private void GenerarNouvelleDirectionPatrouille()
        {
            // Patrouille en cercle autour du point d'origine
            float distanceOrigine = Vector3.Distance(transform.position, pointPatrouilleOrigine);

            if (distanceOrigine > distancePatrouille)
            {
                // Revenir vers l'origine si on s'éloigne trop
                directionPatrouille = (pointPatrouilleOrigine - transform.position).normalized;
            }
            else
            {
                // Direction aléatoire
                directionPatrouille = new Vector3(
                    Random.Range(-1f, 1f),
                    0f,
                    Random.Range(-1f, 1f)
                ).normalized;
            }

            tempsProchainChangement = Time.time + Random.Range(
                tempsChangementDirection * 0.7f,
                tempsChangementDirection * 1.3f
            );
        }

        // Gizmos pour visualiser les rayons de détection et patrouille
        private void OnDrawGizmosSelected()
        {
            // Rayon de détection des proies
            Gizmos.color = Color.red;
            float rayonEffectif = EstAdulte() ? rayonDetectionProie : rayonDetectionProie * 0.7f;
            Gizmos.DrawWireSphere(transform.position, rayonEffectif);

            // Zone de patrouille
            if (Application.isPlaying)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(pointPatrouilleOrigine, distancePatrouille);

                // Direction de patrouille actuelle
                if (etatActuel == EtatRenard.Patrouille)
                {
                    Gizmos.color = Color.blue;
                    Gizmos.DrawRay(transform.position, directionPatrouille * 6f);
                }

                // Ligne vers la cible si en chasse
                if (cibleActuelle != null)
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawLine(transform.position, cibleActuelle.position);
                }
            }
        }
    }
}