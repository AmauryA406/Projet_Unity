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
        [SerializeField] private float seuilReproduction = 120f;
        [SerializeField] private float cooldownReproduction = 15f;
        [SerializeField] private LayerMask layerProie;
        [SerializeField] private GameObject prefabRenard;

        private EtatRenard etatActuel = EtatRenard.Patrouille;
        private Transform cibleActuelle;
        private bool enAttaque = false;
        private bool peutSeReproduire = true;

        protected override void Start()
        {
            energieMax = 150f;
            vitesseBase = 4f;
            perteEnergieParSeconde = 0.5f;

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

        private void TenterReproduction()
        {
            if (energieActuelle >= seuilReproduction && peutSeReproduire && prefabRenard != null)
            {
                StartCoroutine(SeReproduire());
            }
        }

        private IEnumerator SeReproduire()
        {
            peutSeReproduire = false;

            // Créer un nouveau renard à proximité
            Vector3 positionBebe = transform.position + EcosystemManager.GenererPositionAleatoire(2f, 5f);
            EcosystemManager.CreerAnimal(prefabRenard, positionBebe);

            // Coût énergétique important
            energieActuelle -= 60f;

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
            Collider[] proies = Physics.OverlapSphere(transform.position, rayonDetectionProie, layerProie);

            if (proies.Length > 0)
            {
                // Chercher la proie la plus proche
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
            transform.position += direction * vitesseActuelle * Time.deltaTime;

            // Vérifier si on a attrapé la proie
            float distance = Vector3.Distance(transform.position, cibleActuelle.position);
            if (distance < 1f)
            {
                StartCoroutine(AttaquerProie());
            }

            // Abandon si la proie est trop loin
            if (distance > rayonDetectionProie * 1.5f)
            {
                cibleActuelle = null;
                etatActuel = EtatRenard.Patrouille;
            }
        }

        private void MouvementAleatoire()
        {
            Vector3 direction = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
            transform.position += direction * (vitesseActuelle * 0.5f) * Time.deltaTime;
        }

        private IEnumerator AttaquerProie()
        {
            enAttaque = true;
            etatActuel = EtatRenard.Attaque;

            GameObject proie = cibleActuelle.gameObject;

            yield return new WaitForSeconds(tempsAttaque);

            // Manger la proie
            if (proie != null)
            {
                energieActuelle += 70f; // Plus d'énergie qu'avant
                if (energieActuelle > energieMax) energieActuelle = energieMax;

                Destroy(proie);
            }

            // Retour à la patrouille
            enAttaque = false;
            cibleActuelle = null;
            etatActuel = EtatRenard.Patrouille;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, rayonDetectionProie);
        }
    }
}