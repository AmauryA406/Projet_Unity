using UnityEngine;

namespace EcosystemSimulation
{
    public abstract class Animal : MonoBehaviour
    {
        [Header("Stats de base")]
        [SerializeField] protected float energieMax = 100f;
        [SerializeField] protected float vitesseBase = 5f;
        [SerializeField] protected float perteEnergieParSeconde = 1f;

        [Header("État actuel")]
        [SerializeField] protected float energieActuelle;
        [SerializeField] protected float vitesseActuelle;

        protected virtual void Start()
        {
            energieActuelle = energieMax;
            CalculerVitesse();
        }

        protected virtual void Update()
        {
            PerdreEnergie();
            CalculerVitesse();
        }

        protected void PerdreEnergie()
        {
            energieActuelle -= perteEnergieParSeconde * Time.deltaTime;
            if (energieActuelle <= 0)
            {
                Mourir();
            }
        }

        protected void CalculerVitesse()
        {
            float pourcentageEnergie = energieActuelle / energieMax;
            vitesseActuelle = vitesseBase * pourcentageEnergie;
        }

        protected virtual void Mourir()
        {
            Destroy(gameObject);
        }
    }
}