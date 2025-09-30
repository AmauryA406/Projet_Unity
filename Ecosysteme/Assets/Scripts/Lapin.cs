using UnityEngine;
using System.Collections;

namespace EcosystemSimulation
{
    public class Lapin : Animal
    {
        [Header("Stats Lapin")]
        [SerializeField] private float rayonDetectionPredateur = 8f;
        [SerializeField] private float rayonDetectionHerbe = 5f;
        [SerializeField] private float seuilReproduction = 70f;
        [SerializeField] private float cooldownReproduction = 15f;
        [SerializeField] private LayerMask layerPredateur;
        [SerializeField] private LayerMask layerHerbe;
        [SerializeField] private GameObject prefabLapin;

        [Header("Patrouille")]
        [SerializeField] private float tempsChangementDirection = 3f; // Temps avant de changer de direction
        [SerializeField] private float vitessePatrouille = 0.4f; // Facteur de vitesse en patrouille
        [SerializeField] private float tempsArretAleatoire = 1f; // Temps d'arrêt aléatoire

        private bool enFuite = false;
        private bool peutSeReproduire = true;
        private Vector3 directionFuite;
        private Transform herbeTarget;
        private Rigidbody rb;

        // Variables pour la patrouille
        private Vector3 directionPatrouille;
        private float tempsProchainChangement;
        private bool enArret = false;
        private float tempsFinArret;

        protected override void Start()
        {
            // Configuration spécifique aux lapins
            energieMax = 100f;
            vitesseBase = 6f;
            perteEnergieParSeconde = 2f;
            tempsJusquMaturite = 5f;

            rb = GetComponent<Rigidbody>();
            base.Start();

            // Initialiser la patrouille
            InitialiserPatrouille();
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

        private void InitialiserPatrouille()
        {
            // Générer une direction aléatoire pour commencer
            directionPatrouille = new Vector3(
                Random.Range(-1f, 1f),
                0f,
                Random.Range(-1f, 1f)
            ).normalized;

            // Définir le prochain changement de direction
            tempsProchainChangement = Time.time + Random.Range(tempsChangementDirection * 0.5f, tempsChangementDirection * 1.5f);
        }

        protected override void ChangerApparenceMaturite()
        {
            base.ChangerApparenceMaturite();
            transform.localScale = transform.localScale * 1.1f;
            Debug.Log($"Lapin {gameObject.name} a grandi !");
        }

        private void TenterReproduction()
        {
            if (energieActuelle >= seuilReproduction &&
                EstAdulte() &&
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
            float facteurVitesse = EstAdulte() ? 1f : 0.8f;

            if (enFuite)
            {
                // FUITE - Priorité absolue
                direction = directionFuite;
                rb.linearVelocity = new Vector3(
                    direction.x * vitesseBase * facteurVitesse,
                    rb.linearVelocity.y,
                    direction.z * vitesseBase * facteurVitesse
                );
            }
            else if (herbeTarget != null)
            {
                // CHERCHER HERBE - Priorité haute
                direction = (herbeTarget.position - transform.position).normalized;
                rb.linearVelocity = new Vector3(
                    direction.x * vitesseBase * facteurVitesse,
                    rb.linearVelocity.y,
                    direction.z * vitesseBase * facteurVitesse
                );

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
                // PATROUILLE - Comportement par défaut
                MouvementAleatoire();
            }
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
                    // Rester immobile pendant l'arrêt
                    rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
                    return;
                }
            }

            // Vérifier s'il faut changer de direction
            if (Time.time >= tempsProchainChangement)
            {
                // Chance d'avoir un arrêt aléatoire
                if (Random.Range(0f, 1f) < 0.3f) // 30% de chance d'arrêt
                {
                    enArret = true;
                    tempsFinArret = Time.time + Random.Range(0.5f, tempsArretAleatoire);
                    rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
                    return;
                }
                else
                {
                    // Changer de direction
                    directionPatrouille = new Vector3(
                        Random.Range(-1f, 1f),
                        0f,
                        Random.Range(-1f, 1f)
                    ).normalized;

                    tempsProchainChangement = Time.time + Random.Range(
                        tempsChangementDirection * 0.5f,
                        tempsChangementDirection * 1.5f
                    );
                }
            }

            // Appliquer le mouvement de patrouille
            float facteurVitesse = EstAdulte() ? vitessePatrouille : vitessePatrouille * 0.8f;

            rb.linearVelocity = new Vector3(
                directionPatrouille.x * vitesseBase * facteurVitesse,
                rb.linearVelocity.y,
                directionPatrouille.z * vitesseBase * facteurVitesse
            );

            // Optionnel : Debug pour voir la direction de patrouille
            Debug.DrawRay(transform.position, directionPatrouille * 3f, Color.green, 0.1f);
        }

        // Gizmos pour visualiser les rayons de détection
        private void OnDrawGizmosSelected()
        {
            // Rayon de détection des prédateurs
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, rayonDetectionPredateur);

            // Rayon de détection de l'herbe
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, rayonDetectionHerbe);

            // Direction de patrouille actuelle
            if (Application.isPlaying && !enFuite && herbeTarget == null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawRay(transform.position, directionPatrouille * 5f);
            }
        }
    }
}