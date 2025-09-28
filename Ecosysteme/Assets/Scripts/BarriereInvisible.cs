using UnityEngine;

namespace EcosystemSimulation
{
    /// <summary>
    /// Script pour créer des barrières invisibles qui empêchent les animaux de tomber dans le vide
    /// </summary>
    public class BarriereInvisible : MonoBehaviour
    {
        [Header("Configuration de la barrière")]
        [SerializeField] private bool repousserAnimaux = true;
        [SerializeField] private float forceRepulsion = 10f;
        [SerializeField] private bool teleporterAnimaux = false;
        [SerializeField] private Transform centreZone;
        [SerializeField] private bool afficherGizmos = true;

        [Header("Tags d'animaux affectés")]
        [SerializeField] private string[] tagsAnimaux = { "Lapin", "Renard" };

        private void Start()
        {
            // S'assurer que le collider est configuré comme trigger
            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                col.isTrigger = true;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            // Vérifier si l'objet qui touche est un animal
            if (EstUnAnimal(other.tag))
            {
                Debug.Log($"Animal {other.name} a touché la barrière !");

                if (repousserAnimaux)
                {
                    RepousserAnimal(other);
                }
                else if (teleporterAnimaux)
                {
                    TeleporterAnimal(other);
                }
            }
        }

        private bool EstUnAnimal(string tag)
        {
            foreach (string animalTag in tagsAnimaux)
            {
                if (tag == animalTag)
                {
                    return true;
                }
            }
            return false;
        }

        private void RepousserAnimal(Collider animalCollider)
        {
            Rigidbody rb = animalCollider.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // Calculer la direction de répulsion (vers le centre de la zone)
                Vector3 directionRepulsion;

                if (centreZone != null)
                {
                    directionRepulsion = (centreZone.position - animalCollider.transform.position).normalized;
                }
                else
                {
                    // Si pas de centre défini, repousser vers l'origine
                    directionRepulsion = (Vector3.zero - animalCollider.transform.position).normalized;
                }

                // Appliquer la force de répulsion
                rb.AddForce(directionRepulsion * forceRepulsion, ForceMode.Impulse);

                Debug.Log($"Animal {animalCollider.name} repoussé vers le centre !");
            }
        }

        private void TeleporterAnimal(Collider animalCollider)
        {
            if (centreZone != null)
            {
                // Téléporter l'animal près du centre avec une petite variation aléatoire
                Vector3 nouvellePosition = centreZone.position + new Vector3(
                    Random.Range(-3f, 3f),
                    1f,
                    Random.Range(-3f, 3f)
                );

                animalCollider.transform.position = nouvellePosition;

                // Réinitialiser la vélocité pour éviter qu'il continue à voler
                Rigidbody rb = animalCollider.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.linearVelocity = Vector3.zero;
                }

                Debug.Log($"Animal {animalCollider.name} téléporté au centre !");
            }
        }

        private void OnDrawGizmos()
        {
            if (afficherGizmos)
            {
                Collider col = GetComponent<Collider>();
                if (col != null)
                {
                    Gizmos.color = Color.red;
                    Gizmos.matrix = transform.localToWorldMatrix;

                    if (col is BoxCollider box)
                    {
                        Gizmos.DrawWireCube(box.center, box.size);
                    }
                    else if (col is SphereCollider sphere)
                    {
                        Gizmos.DrawWireSphere(sphere.center, sphere.radius);
                    }
                }
            }
        }
    }
}