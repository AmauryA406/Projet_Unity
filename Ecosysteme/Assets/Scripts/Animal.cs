// ========== CLASSE ANIMAL.CS MODIFIÉE ==========
using UnityEngine;

namespace EcosystemSimulation
{
    public abstract class Animal : MonoBehaviour
    {
        [Header("Stats de base")]
        [SerializeField] protected float energieMax = 100f;
        [SerializeField] protected float vitesseBase = 5f;
        [SerializeField] protected float perteEnergieParSeconde = 1f;

        [Header("Système de Maturité")]
        [SerializeField] protected float tempsJusquMaturite = 8f; // Temps avant de pouvoir se reproduire
        [SerializeField] protected bool estAdulte = false; // Statut de maturité

        protected float energieActuelle;
        protected float ageActuel = 0f;
        private float tempsDerniereUpdate;

        protected virtual void Start()
        {
            tempsDerniereUpdate = Time.time;

            // Énergie initiale réduite pour les jeunes
            energieActuelle = energieMax * 0.4f; // 40% de l'énergie max au spawn

            // Démarrer le processus de maturation
            StartCoroutine(ProcessusMaturation());
        }

        protected virtual void Update()
        {
            float tempsEcoule = Time.time - tempsDerniereUpdate;
            tempsDerniereUpdate = Time.time;

            // Vieillir l'animal
            ageActuel += tempsEcoule;

            // Perdre de l'énergie avec le temps
            energieActuelle -= perteEnergieParSeconde * tempsEcoule;
            energieActuelle = Mathf.Clamp(energieActuelle, 0f, energieMax);

            // Mourir si plus d'énergie
            if (energieActuelle <= 0f)
            {
                Debug.Log($"{gameObject.name} est mort de faim à l'âge de {ageActuel:F1} secondes");
                Destroy(gameObject);
            }
        }

        // Coroutine pour gérer la maturation
        private System.Collections.IEnumerator ProcessusMaturation()
        {
            Debug.Log($"{gameObject.name} est né ! Maturité dans {tempsJusquMaturite} secondes.");

            yield return new WaitForSeconds(tempsJusquMaturite);

            estAdulte = true;
            Debug.Log($"{gameObject.name} a atteint la maturité sexuelle !");

            // Optionnel : changer l'apparence pour montrer la maturité
            ChangerApparenceMaturite();
        }

        // Méthode virtuelle pour changer l'apparence à la maturité
        protected virtual void ChangerApparenceMaturite()
        {
            // Les classes filles peuvent redéfinir cette méthode
            // Par exemple : changer la couleur, la taille, etc.
            if (TryGetComponent<Renderer>(out Renderer renderer))
            {
                // Rendre la couleur plus saturée pour les adultes
                Color couleurActuelle = renderer.material.color;
                renderer.material.color = couleurActuelle * 1.2f;
            }
        }

        // Méthode pour vérifier si l'animal peut se reproduire
        public bool PeutSeReproduire()
        {
            return estAdulte;
        }

        // Méthode pour récupérer l'âge
        public float ObtenirAge()
        {
            return ageActuel;
        }

        // Méthode pour obtenir le statut de maturité
        public bool EstAdulte()
        {
            return estAdulte;
        }

        // Méthode pour récupérer de l'énergie (manger)
        public virtual void GagnerEnergie(float quantite)
        {
            energieActuelle += quantite;
            energieActuelle = Mathf.Clamp(energieActuelle, 0f, energieMax);
        }

        // Méthodes abstraites à implémenter par les classes filles
        protected abstract void Deplacer();
        protected abstract void MouvementAleatoire();
    }
}