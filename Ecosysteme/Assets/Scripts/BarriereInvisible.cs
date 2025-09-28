using UnityEngine;

namespace EcosystemSimulation
{
    /// <summary>
    /// Script pour cr�er des barri�res invisibles qui emp�chent les animaux de tomber dans le vide
    /// </summary>
    public class BarriereInvisible : MonoBehaviour
    {
        [Header("Configuration de la barri�re")]
        [SerializeField] private bool repousserAnimaux = true;
        [SerializeField] private float forceRepulsion = 10f;
        [SerializeField] private bool teleporterAnimaux = false;
        [SerializeField] private Transform centreZone;
        [SerializeField] private bool afficherGizmos = true;

        [Header("Tags d'animaux affect�s")]
        [SerializeField] private string[] tagsAnimaux = { "Lapin", "Renard" };

        private void Start()
        {
            // S'assurer que le collider est configur� comme trigger
            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                col.isTrigger = true;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            // V�rifier si l'objet qui touche est un animal
            if (EstUnAnimal(other.tag))
            {
                Debug.Log($"Animal {other.name} a touch� la barri�re !");

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
                // Calculer la direction de r�pulsion (vers le centre de la zone)
                Vector3 directionRepulsion;

                if (centreZone != null)
                {
                    directionRepulsion = (centreZone.position - animalCollider.transform.position).normalized;
                }
                else
                {
                    // Si pas de centre d�fini, repousser vers l'origine
                    directionRepulsion = (Vector3.zero - animalCollider.transform.position).normalized;
                }

                // Appliquer la force de r�pulsion
                rb.AddForce(directionRepulsion * forceRepulsion, ForceMode.Impulse);

                Debug.Log($"Animal {animalCollider.name} repouss� vers le centre !");
            }
        }

        private void TeleporterAnimal(Collider animalCollider)
        {
            if (centreZone != null)
            {
                // T�l�porter l'animal pr�s du centre avec une petite variation al�atoire
                Vector3 nouvellePosition = centreZone.position + new Vector3(
                    Random.Range(-3f, 3f),
                    1f,
                    Random.Range(-3f, 3f)
                );

                animalCollider.transform.position = nouvellePosition;

                // R�initialiser la v�locit� pour �viter qu'il continue � voler
                Rigidbody rb = animalCollider.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.linearVelocity = Vector3.zero;
                }

                Debug.Log($"Animal {animalCollider.name} t�l�port� au centre !");
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