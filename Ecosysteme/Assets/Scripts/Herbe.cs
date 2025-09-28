
// ========== HERBE.CS - SCRIPT COMPLET ==========
using UnityEngine;
using System.Collections;

namespace EcosystemSimulation
{
    public class Herbe : MonoBehaviour
    {
        [Header("Propriétés Herbe")]
        [SerializeField] private float tempsRepousse = 5f;
        [SerializeField] private float energieFournie = 30f;
        [SerializeField] private bool estMangeable = true;

        private Renderer herbeRenderer;
        private Collider herbeCollider;

        void Start()
        {
            herbeRenderer = GetComponent<Renderer>();
            herbeCollider = GetComponent<Collider>();
        }

        // Méthode pour vérifier si l'herbe peut être mangée
        public bool PeutEtreMangee()
        {
            return estMangeable;
        }

        // Méthode pour être mangée (nouvelle version)
        public void EtreMangee()
        {
            if (!estMangeable) return;

            estMangeable = false;

            // Cacher l'herbe visuellement
            if (herbeRenderer != null)
                herbeRenderer.enabled = false;

            // Désactiver le collider
            if (herbeCollider != null)
                herbeCollider.enabled = false;

            // Démarrer la repousse
            StartCoroutine(Repousser());
        }

        // Méthode pour obtenir l'énergie fournie
        public float ObtenirEnergieFournie()
        {
            return energieFournie;
        }

        // Ancienne méthode pour compatibilité (si vous l'utilisiez)
        public float Consommer()
        {
            EtreMangee();
            return energieFournie;
        }

        // Coroutine pour faire repousser l'herbe
        private IEnumerator Repousser()
        {
            yield return new WaitForSeconds(tempsRepousse);

            // Réactiver l'herbe
            estMangeable = true;

            if (herbeRenderer != null)
                herbeRenderer.enabled = true;

            if (herbeCollider != null)
                herbeCollider.enabled = true;
        }
    }
}