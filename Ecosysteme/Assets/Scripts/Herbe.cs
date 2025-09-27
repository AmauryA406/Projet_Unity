using UnityEngine;
using System.Collections;

namespace EcosystemSimulation
{
    public class Herbe : MonoBehaviour
    {
        [Header("Paramètres Herbe")]
        [SerializeField] private float tempsRepousse = 5f;
        [SerializeField] private float energieFournie = 30f;

        private bool estConsommee = false;
        private Renderer herbeRenderer;
        private Collider herbeCollider;

        private void Start()
        {
            herbeRenderer = GetComponent<Renderer>();
            herbeCollider = GetComponent<Collider>();
        }

        public bool PeutEtreMangee()
        {
            return !estConsommee;
        }

        public float Consommer()
        {
            if (estConsommee) return 0f;

            estConsommee = true;
            herbeRenderer.enabled = false;
            herbeCollider.enabled = false;

            StartCoroutine(Repousser());
            return energieFournie;
        }

        private IEnumerator Repousser()
        {
            yield return new WaitForSeconds(tempsRepousse);

            estConsommee = false;
            herbeRenderer.enabled = true;
            herbeCollider.enabled = true;
        }
    }
}