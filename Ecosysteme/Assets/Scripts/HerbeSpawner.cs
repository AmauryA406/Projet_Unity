using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace EcosystemSimulation
{
    public class HerbeSpawner : MonoBehaviour
    {
        [Header("Zone de génération")]
        [SerializeField] private float rayonZone = 20f;
        [SerializeField] private Vector3 centreZone = Vector3.zero;

        [Header("Paramètres de génération")]
        [SerializeField] private GameObject prefabHerbe;
        [SerializeField] private int nombreHerbeInitial = 30;
        [SerializeField] private int nombreHerbeMax = 50;
        [SerializeField] private float intervalleGeneration = 3f;

        [Header("Contraintes")]
        [SerializeField] private float distanceMinimaleEntreHerbes = 2f;
        [SerializeField] private LayerMask layersEviter; // Pour éviter de spawner sur les animaux

        private List<GameObject> herbesActives = new List<GameObject>();

        private void Start()
        {
            // Génération initiale
            for (int i = 0; i < nombreHerbeInitial; i++)
            {
                TenterGenererHerbe();
            }

            // Démarrer la génération continue
            StartCoroutine(GenerationContinue());
        }

        private void TenterGenererHerbe()
        {
            if (herbesActives.Count >= nombreHerbeMax) return;

            Vector3 positionAleatoire = GenererPositionValide();

            if (positionAleatoire != Vector3.zero)
            {
                GameObject nouvelleHerbe = EcosystemManager.CreerAnimal(prefabHerbe, positionAleatoire);
                if (nouvelleHerbe != null)
                {
                    herbesActives.Add(nouvelleHerbe);

                    // S'abonner à la destruction de l'herbe (optionnel)
                    StartCoroutine(SurveillerHerbe(nouvelleHerbe));
                }
            }
        }

        private Vector3 GenererPositionValide()
        {
            int tentativesMax = 20; // Éviter les boucles infinies

            for (int i = 0; i < tentativesMax; i++)
            {
                // Position aléatoire dans le cercle
                Vector2 positionCircle = Random.insideUnitCircle * rayonZone;
                Vector3 positionCandidate = centreZone + new Vector3(positionCircle.x, 0.05f, positionCircle.y);

                if (EstPositionValide(positionCandidate))
                {
                    return positionCandidate;
                }
            }

            return Vector3.zero; // Aucune position valide trouvée
        }

        private bool EstPositionValide(Vector3 position)
        {
            // Vérifier s'il y a déjà quelque chose à cet endroit
            Collider[] objetsProches = Physics.OverlapSphere(position, distanceMinimaleEntreHerbes, layersEviter);
            if (objetsProches.Length > 0) return false;

            // Vérifier la distance avec les autres herbes
            foreach (GameObject herbe in herbesActives)
            {
                if (herbe != null)
                {
                    float distance = Vector3.Distance(position, herbe.transform.position);
                    if (distance < distanceMinimaleEntreHerbes)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private IEnumerator GenerationContinue()
        {
            while (true)
            {
                yield return new WaitForSeconds(intervalleGeneration);

                // Nettoyer la liste des herbes détruites
                EcosystemManager.NettoyerListeAnimaux(ConvertirListeGameObject());

                // Tenter de générer une nouvelle herbe
                TenterGenererHerbe();
            }
        }

        private IEnumerator SurveillerHerbe(GameObject herbe)
        {
            // Attendre que l'herbe soit détruite
            while (herbe != null)
            {
                yield return new WaitForSeconds(1f);
            }

            // Retirer de la liste
            herbesActives.Remove(herbe);
        }

        private List<Transform> ConvertirListeGameObject()
        {
            List<Transform> transforms = new List<Transform>();
            foreach (GameObject herbe in herbesActives)
            {
                if (herbe != null)
                {
                    transforms.Add(herbe.transform);
                }
            }
            return transforms;
        }

        private void OnDrawGizmosSelected()
        {
            // Visualiser la zone de génération
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(centreZone, rayonZone);

            // Visualiser le centre
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(centreZone, 0.5f);
        }
    }
}