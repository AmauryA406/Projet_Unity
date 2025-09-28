// ========== CLASSE ANIMAL.CS MODIFI�E ==========
using UnityEngine;

namespace EcosystemSimulation
{
    public abstract class Animal : MonoBehaviour
    {
        [Header("Stats de base")]
        [SerializeField] protected float energieMax = 100f;
        [SerializeField] protected float vitesseBase = 5f;
        [SerializeField] protected float perteEnergieParSeconde = 1f;

        [Header("Syst�me de Maturit�")]
        [SerializeField] protected float tempsJusquMaturite = 8f; // Temps avant de pouvoir se reproduire
        [SerializeField] protected bool estAdulte = false; // Statut de maturit�

        protected float energieActuelle;
        protected float ageActuel = 0f;
        private float tempsDerniereUpdate;

        protected virtual void Start()
        {
            tempsDerniereUpdate = Time.time;

            // �nergie initiale r�duite pour les jeunes
            energieActuelle = energieMax * 0.4f; // 40% de l'�nergie max au spawn

            // D�marrer le processus de maturation
            StartCoroutine(ProcessusMaturation());
        }

        protected virtual void Update()
        {
            float tempsEcoule = Time.time - tempsDerniereUpdate;
            tempsDerniereUpdate = Time.time;

            // Vieillir l'animal
            ageActuel += tempsEcoule;

            // Perdre de l'�nergie avec le temps
            energieActuelle -= perteEnergieParSeconde * tempsEcoule;
            energieActuelle = Mathf.Clamp(energieActuelle, 0f, energieMax);

            // Mourir si plus d'�nergie
            if (energieActuelle <= 0f)
            {
                Debug.Log($"{gameObject.name} est mort de faim � l'�ge de {ageActuel:F1} secondes");
                Destroy(gameObject);
            }
        }

        // Coroutine pour g�rer la maturation
        private System.Collections.IEnumerator ProcessusMaturation()
        {
            Debug.Log($"{gameObject.name} est n� ! Maturit� dans {tempsJusquMaturite} secondes.");

            yield return new WaitForSeconds(tempsJusquMaturite);

            estAdulte = true;
            Debug.Log($"{gameObject.name} a atteint la maturit� sexuelle !");

            // Optionnel : changer l'apparence pour montrer la maturit�
            ChangerApparenceMaturite();
        }

        // M�thode virtuelle pour changer l'apparence � la maturit�
        protected virtual void ChangerApparenceMaturite()
        {
            // Les classes filles peuvent red�finir cette m�thode
            // Par exemple : changer la couleur, la taille, etc.
            if (TryGetComponent<Renderer>(out Renderer renderer))
            {
                // Rendre la couleur plus satur�e pour les adultes
                Color couleurActuelle = renderer.material.color;
                renderer.material.color = couleurActuelle * 1.2f;
            }
        }

        // M�thode pour v�rifier si l'animal peut se reproduire
        public bool PeutSeReproduire()
        {
            return estAdulte;
        }

        // M�thode pour r�cup�rer l'�ge
        public float ObtenirAge()
        {
            return ageActuel;
        }

        // M�thode pour obtenir le statut de maturit�
        public bool EstAdulte()
        {
            return estAdulte;
        }

        // M�thode pour r�cup�rer de l'�nergie (manger)
        public virtual void GagnerEnergie(float quantite)
        {
            energieActuelle += quantite;
            energieActuelle = Mathf.Clamp(energieActuelle, 0f, energieMax);
        }

        // M�thodes abstraites � impl�menter par les classes filles
        protected abstract void Deplacer();
        protected abstract void MouvementAleatoire();
    }
}