// ========== LAPIN.CS CORRIGÉ POUR COMPATIBILITÉ ==========
using UnityEngine;
using System.Collections;

namespace EcosystemSimulation
{
    public class Lapin : Animal
    {
        [Header("Stats Lapin")]
        [SerializeField] private float rayonDetectionPredateur = 8f;
        [SerializeField] private float rayonDetectionHerbe = 5f;
        [SerializeField] private float seuilReproduction = 70f; // Réduit pour le nouveau système
        [SerializeField] private float cooldownReproduction = 15f;
        [SerializeField] private LayerMask layerPredateur;
        [SerializeField] private LayerMask layerHerbe;
        [SerializeField] private GameObject prefabLapin;

        private bool enFuite = false;
        private bool peutSeReproduire = true;
        private Vector3 directionFuite;
        private Transform herbeTarget;
        private Rigidbody rb;

        protected override void Start()
        {
            // Configuration spécifique aux lapins
            energieMax = 100f;
            vitesseBase = 6f;
            perteEnergieParSeconde = 2f;
            tempsJusquMaturite = 5f; // Lapins maturent plus vite que les renards

            rb = GetComponent<Rigidbody>();
            base.Start(); // Important : appeler APRÈS avoir configuré les valeurs
        }

        protected override void Update()
        {
            base.Update();

            DetecterPredateur();
            if (!enFuite)
            {
                ChercherHerbe();
                TenterReproduction();
            }
            Deplacer();
        }

        protected override void ChangerApparenceMaturite()
        {
            base.ChangerApparenceMaturite();

            // Changer la taille pour montrer la maturité
            transform.localScale = transform.localScale * 1.1f;

            Debug.Log($"Lapin {gameObject.name} a grandi !");
        }

        private void TenterReproduction()
        {
            // Vérifier TOUTES les conditions : énergie, maturité, cooldown, population
            if (energieActuelle >= seuilReproduction &&
                EstAdulte() && // Vérifier la maturité
                peutSeReproduire &&
                prefabLapin != null)
            {
                if (EcosystemManager.PeutSeReproduire("Lapin", 15))
                {
                    StartCoroutine(SeReproduire());
                }
            }
        }

        private IEnumerator SeReproduire()
        {
            peutSeReproduire = false;

            Debug.Log($"Lapin {gameObject.name} (âge: {ObtenirAge():F1}s) se reproduit !");

            Vector3 positionBebe = transform.position + EcosystemManager.GenererPositionAleatoire(1f, 3f);
            GameObject nouveauLapin = EcosystemManager.CreerAnimal(prefabLapin, positionBebe);

            if (nouveauLapin != null)
            {
                nouveauLapin.tag = "Lapin";
                nouveauLapin.name = $"Lapin_Enfant_{Random.Range(1000, 9999)}";
            }

            // Coût énergétique de la reproduction
            energieActuelle -= 30f;

            yield return new WaitForSeconds(cooldownReproduction);
            peutSeReproduire = true;
        }

        private void DetecterPredateur()
        {
            Collider[] predateurs = Physics.OverlapSphere(transform.position, rayonDetectionPredateur, layerPredateur);

            if (predateurs.Length > 0)
            {
                Vector3 directionDanger = predateurs[0].transform.position - transform.position;
                directionFuite = -directionDanger.normalized;
                enFuite = true;
                herbeTarget = null;
            }
            else
            {
                enFuite = false;
            }
        }

        private void ChercherHerbe()
        {
            if (herbeTarget != null) return;

            Collider[] herbes = Physics.OverlapSphere(transform.position, rayonDetectionHerbe, layerHerbe);

            if (herbes.Length > 0)
            {
                foreach (Collider herbeCollider in herbes)
                {
                    Herbe herbe = herbeCollider.GetComponent<Herbe>();
                    if (herbe != null && herbe.PeutEtreMangee())
                    {
                        herbeTarget = herbeCollider.transform;
                        break;
                    }
                }
            }
        }

        protected override void Deplacer()
        {
            if (rb == null) return;

            Vector3 direction = Vector3.zero;

            if (enFuite)
            {
                direction = directionFuite;
                // Les jeunes lapins courent moins vite
                float facteurVitesse = EstAdulte() ? 1f : 0.8f;
                rb.linearVelocity = new Vector3(direction.x * vitesseBase * facteurVitesse, rb.linearVelocity.y, direction.z * vitesseBase * facteurVitesse);
            }
            else if (herbeTarget != null)
            {
                direction = (herbeTarget.position - transform.position).normalized;
                rb.linearVelocity = new Vector3(direction.x * vitesseBase, rb.linearVelocity.y, direction.z * vitesseBase);

                // Manger l'herbe si assez proche
                if (Vector3.Distance(transform.position, herbeTarget.position) < 2f)
                {
                    Herbe herbe = herbeTarget.GetComponent<Herbe>();
                    if (herbe != null && herbe.PeutEtreMangee())
                    {
                        herbe.EtreMangee();
                        GagnerEnergie(herbe.ObtenirEnergieFournie());
                        herbeTarget = null;
                        Debug.Log($"Lapin {gameObject.name} a mangé de l'herbe !");
                    }
                }
            }
            else
            {
                MouvementAleatoire();
            }
        }

        protected override void MouvementAleatoire()
        {
            if (rb == null) return;

            Vector3 directionAleatoire = new Vector3(
                Random.Range(-1f, 1f),
                0f,
                Random.Range(-1f, 1f)
            ).normalized;

            // Les jeunes animaux bougent moins
            float facteurVitesse = EstAdulte() ? 0.5f : 0.3f;

            rb.linearVelocity = new Vector3(
                directionAleatoire.x * vitesseBase * facteurVitesse,
                rb.linearVelocity.y,
                directionAleatoire.z * vitesseBase * facteurVitesse
            );
        }

        // Gizmos pour visualiser les rayons de détection
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, rayonDetectionPredateur);

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, rayonDetectionHerbe);
        }
    }
}