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
        [SerializeField] private float tempsArretAleatoire = 2f; // Arr�ts plus longs
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
            // Configuration sp�cifique aux renards
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
            // G�n�rer une direction al�atoire pour la patrouille
            directionPatrouille = new Vector3(
                Random.Range(-1f, 1f),
                0f,
                Random.Range(-1f, 1f)
            ).normalized;

            // D�finir le prochain changement de direction
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

            Debug.Log($"Renard {gameObject.name} a atteint sa maturit� de pr�dateur !");
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
            Debug.Log($"Renard {gameObject.name} (�ge: {ObtenirAge():F1}s) se reproduit !");

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
            // Les jeunes renards ont une d�tection r�duite
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
                Debug.Log($"Renard {gameObject.name} a d�tect� une proie : {cibleActuelle.name}");
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
                // R�ussir l'attaque
                Destroy(cibleActuelle.gameObject);
                GagnerEnergie(40f);
                Debug.Log($"Renard {gameObject.name} a attrap� sa proie !");
            }
            else
            {
                Debug.Log($"Renard {gameObject.name} a rat� son attaque");
            }

            enAttaque = false;
            cibleActuelle = null;
            etatActuel = EtatRenard.Patrouille;
        }

        protected override void Deplacer()
        {
            // La logique de mouvement est d�j� dans GererComportement
        }

        protected override void MouvementAleatoire()
        {
            if (rb == null) return;

            // V�rifier si on est en arr�t
            if (enArret)
            {
                if (Time.time >= tempsFinArret)
                {
                    enArret = false;
                    InitialiserPatrouille(); // Nouvelle direction apr�s l'arr�t
                }
                else
                {
                    // Rester immobile pendant l'arr�t (comportement de chasseur � l'aff�t)
                    rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
                    return;
                }
            }

            // V�rifier s'il faut changer de direction
            if (Time.time >= tempsProchainChangement)
            {
                // Chance d'avoir un arr�t al�atoire (renards s'arr�tent pour �couter/observer)
                if (Random.Range(0f, 1f) < 0.4f) // 40% de chance d'arr�t
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
                // Revenir vers l'origine si on s'�loigne trop
                directionPatrouille = (pointPatrouilleOrigine - transform.position).normalized;
            }
            else
            {
                // Direction al�atoire
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

        // Gizmos pour visualiser les rayons de d�tection et patrouille
        private void OnDrawGizmosSelected()
        {
            // Rayon de d�tection des proies
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