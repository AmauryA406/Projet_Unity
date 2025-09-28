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
        [SerializeField] private float seuilReproduction = 130f;
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
            energieMax = 150f;
            vitesseBase = 4f;
            perteEnergieParSeconde = 0.5f;

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

        private void TenterReproduction()
        {
            if (energieActuelle >= seuilReproduction && peutSeReproduire && prefabRenard != null)
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

            Vector3 positionBebe = transform.position + EcosystemManager.GenererPositionAleatoire(2f, 5f);
            EcosystemManager.CreerAnimal(prefabRenard, positionBebe);

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

            Vector3 mouvement = direction * vitesseActuelle;
            mouvement.y = rb.linearVelocity.y;
            rb.linearVelocity = mouvement;

            float distance = Vector3.Distance(transform.position, cibleActuelle.position);
            if (distance < 1f)
            {
                StartCoroutine(AttaquerProie());
            }

            if (distance > rayonDetectionProie * 1.5f)
            {
                cibleActuelle = null;
                etatActuel = EtatRenard.Patrouille;
            }
        }

        private void MouvementAleatoire()
        {
            Vector3 direction = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;

            Vector3 mouvement = direction * (vitesseActuelle * 0.5f);
            mouvement.y = rb.linearVelocity.y;
            rb.linearVelocity = mouvement;
        }

        private IEnumerator AttaquerProie()
        {
            enAttaque = true;
            etatActuel = EtatRenard.Attaque;

            GameObject proie = cibleActuelle.gameObject;

            yield return new WaitForSeconds(tempsAttaque);

            if (proie != null)
            {
                energieActuelle += 70f;
                if (energieActuelle > energieMax) energieActuelle = energieMax;

                Destroy(proie);
            }

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