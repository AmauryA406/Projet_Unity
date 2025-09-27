using UnityEngine;
using System.Collections;

namespace EcosystemSimulation
{
    public class Lapin : Animal
    {
        [Header("Stats Lapin")]
        [SerializeField] private float rayonDetectionPredateur = 8f;
        [SerializeField] private float rayonDetectionHerbe = 5f;
        [SerializeField] private float seuilReproduction = 80f;
        [SerializeField] private float cooldownReproduction = 10f;
        [SerializeField] private LayerMask layerPredateur;
        [SerializeField] private LayerMask layerHerbe;
        [SerializeField] private GameObject prefabLapin;

        private bool enFuite = false;
        private bool peutSeReproduire = true;
        private Vector3 directionFuite;
        private Transform herbeTarget;

        protected override void Start()
        {
            energieMax = 100f;
            vitesseBase = 6f;
            perteEnergieParSeconde = 2f;

            base.Start();
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

        private void DetecterPredateur()
        {
            Collider[] predateurs = Physics.OverlapSphere(transform.position, rayonDetectionPredateur, layerPredateur);

            if (predateurs.Length > 0)
            {
                Vector3 directionDanger = predateurs[0].transform.position - transform.position;
                directionFuite = -directionDanger.normalized;
                enFuite = true;
                herbeTarget = null; // Abandonner la recherche d'herbe
            }
            else
            {
                enFuite = false;
            }
        }

        private void ChercherHerbe()
        {
            if (herbeTarget != null) return; // Déjà une cible

            Collider[] herbes = Physics.OverlapSphere(transform.position, rayonDetectionHerbe, layerHerbe);

            if (herbes.Length > 0)
            {
                // Trouver l'herbe la plus proche qui peut être mangée
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

        private void TenterReproduction()
        {
            if (energieActuelle >= seuilReproduction && peutSeReproduire && prefabLapin != null)
            {
                StartCoroutine(SeReproduire());
            }
        }

        private IEnumerator SeReproduire()
        {
            peutSeReproduire = false;

            // Créer un nouveau lapin à proximité
            Vector3 positionBebe = transform.position + EcosystemManager.GenererPositionAleatoire(1f, 3f);
            EcosystemManager.CreerAnimal(prefabLapin, positionBebe);

            // Coût énergétique
            energieActuelle -= 40f;

            yield return new WaitForSeconds(cooldownReproduction);
            peutSeReproduire = true;
        }

        private void Deplacer()
        {
            Vector3 direction;

            if (enFuite)
            {
                direction = directionFuite;
            }
            else if (herbeTarget != null)
            {
                // Aller vers l'herbe
                direction = (herbeTarget.position - transform.position).normalized;

                // Vérifier si on a atteint l'herbe
                if (Vector3.Distance(transform.position, herbeTarget.position) < 1f)
                {
                    Herbe herbe = herbeTarget.GetComponent<Herbe>();
                    if (herbe != null && herbe.PeutEtreMangee())
                    {
                        float energieGagnee = herbe.Consommer();
                        energieActuelle += energieGagnee;
                        if (energieActuelle > energieMax) energieActuelle = energieMax;
                    }
                    herbeTarget = null;
                }
            }
            else
            {
                // Mouvement aléatoire
                direction = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
            }

            transform.position += direction * vitesseActuelle * Time.deltaTime;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, rayonDetectionPredateur);

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, rayonDetectionHerbe);
        }
    }
}