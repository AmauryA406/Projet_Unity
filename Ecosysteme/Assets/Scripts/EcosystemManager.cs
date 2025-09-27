using UnityEngine;
using System.Collections.Generic;

namespace EcosystemSimulation
{
    public static class EcosystemManager
    {
        // Nouvelles fonctions de comptage
        public static int CompterAnimaux(string tag)
        {
            GameObject[] animaux = GameObject.FindGameObjectsWithTag(tag);
            return animaux.Length;
        }

        public static bool PeutSeReproduire(string typeAnimal, int populationMax)
        {
            int populationActuelle = CompterAnimaux(typeAnimal);
            return populationActuelle < populationMax;
        }

        // Fonctions existantes...
        public static Vector3 GenererPositionAleatoire(float rayonMin, float rayonMax)
        {
            Vector2 positionCircle = Random.insideUnitCircle.normalized * Random.Range(rayonMin, rayonMax);
            return new Vector3(positionCircle.x, 0.5f, positionCircle.y);
        }

        public static float CalculerDistance(Transform animal1, Transform animal2)
        {
            if (animal1 == null || animal2 == null) return float.MaxValue;
            return Vector3.Distance(animal1.position, animal2.position);
        }

        public static Transform TrouverAnimalLePlusProche(Transform source, List<Transform> animaux)
        {
            if (animaux == null || animaux.Count == 0) return null;

            Transform lePlusProche = null;
            float distanceMin = float.MaxValue;

            foreach (Transform animal in animaux)
            {
                if (animal != source && animal != null)
                {
                    float distance = CalculerDistance(source, animal);
                    if (distance < distanceMin)
                    {
                        distanceMin = distance;
                        lePlusProche = animal;
                    }
                }
            }

            return lePlusProche;
        }

        public static GameObject CreerAnimal(GameObject prefab, Vector3 position)
        {
            if (prefab == null) return null;
            return Object.Instantiate(prefab, position, Quaternion.identity);
        }

        public static void NettoyerListeAnimaux(List<Transform> listeAnimaux)
        {
            if (listeAnimaux == null) return;

            for (int i = listeAnimaux.Count - 1; i >= 0; i--)
            {
                if (listeAnimaux[i] == null)
                {
                    listeAnimaux.RemoveAt(i);
                }
            }
        }
    }
}