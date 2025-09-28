
// ========== HERBE.CS - SCRIPT COMPLET ==========
using UnityEngine;
using System.Collections;

namespace EcosystemSimulation
{
    public class Herbe : MonoBehaviour
    {
        [Header("Propri�t�s Herbe")]
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

        // M�thode pour v�rifier si l'herbe peut �tre mang�e
        public bool PeutEtreMangee()
        {
            return estMangeable;
        }

        // M�thode pour �tre mang�e (nouvelle version)
        public void EtreMangee()
        {
            if (!estMangeable) return;

            estMangeable = false;

            // Cacher l'herbe visuellement
            if (herbeRenderer != null)
                herbeRenderer.enabled = false;

            // D�sactiver le collider
            if (herbeCollider != null)
                herbeCollider.enabled = false;

            // D�marrer la repousse
            StartCoroutine(Repousser());
        }

        // M�thode pour obtenir l'�nergie fournie
        public float ObtenirEnergieFournie()
        {
            return energieFournie;
        }

        // Ancienne m�thode pour compatibilit� (si vous l'utilisiez)
        public float Consommer()
        {
            EtreMangee();
            return energieFournie;
        }

        // Coroutine pour faire repousser l'herbe
        private IEnumerator Repousser()
        {
            yield return new WaitForSeconds(tempsRepousse);

            // R�activer l'herbe
            estMangeable = true;

            if (herbeRenderer != null)
                herbeRenderer.enabled = true;

            if (herbeCollider != null)
                herbeCollider.enabled = true;
        }
    }
}