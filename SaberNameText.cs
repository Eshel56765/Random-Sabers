using System.Collections;
using UnityEngine;

namespace RandomSabers
{
    class SaberNameText : MonoBehaviour
    {
        public static void DisplayText(string SaberName)
        {
            SaberNameText DisplayText = new GameObject("DisplayText", typeof(MeshRenderer)).AddComponent<SaberNameText>();
            GameObject origin = GameObject.Find("Origin");
            DisplayText.transform.position = origin.transform.position + new Vector3(0, 1.7f, 2);
            DisplayText.textMesh = DisplayText.gameObject.AddComponent<TextMesh>();
            DisplayText.textMesh.text = "Current Saber:\n" + SaberName;
            DisplayText.SetTMParams();
            DisplayText.StartCoroutine(DisplayText.Animation());
        }

        private void SetTMParams()
        {
            textMesh.color = Color.white;
            textMesh.fontSize = 200;
            textMesh.characterSize = 1;
            transform.localScale = Vector3.one / 100f;

            textMesh.alignment = TextAlignment.Center;
            textMesh.anchor = TextAnchor.MiddleCenter;
        }

        private TextMesh textMesh;
        private Vector3 MovementDirection = new Vector3(0, 0.5f, 0.9f);
        private float MovementTime => 1.7f;
        private float MovementSpeed => 1.2f;
        private float DisappearingTime => 0.8f;

        private IEnumerator Animation()
        {
            float startTime = Time.time;
            while (Time.time < startTime + MovementTime)
            {
                transform.position = transform.position + MovementDirection * (Time.deltaTime * MovementSpeed);
                yield return null;
            }
            startTime = Time.time;
            while (Time.time < startTime + DisappearingTime)
            {
                transform.position = transform.position + MovementDirection * (Time.deltaTime * MovementSpeed);
                float a = (Time.time - startTime) / DisappearingTime;
                textMesh.color = new Color(1, 1, 1, 1 - a);
                yield return null;
            }
            Destroy(gameObject);
        }

        /*public void DisplayHeirarchy(Scene scene)
        {
            foreach (GameObject obj in scene.GetRootGameObjects())
            {
                PrintChildren(obj.transform, 0);
            }
        }

        private void PrintChildren(Transform transform, int Indentation)
        {
            for (int i = 0; i < Indentation; i++)
                Console.Write('\t');
            Console.WriteLine(transform.name);
            foreach (Transform child in transform)
                PrintChildren(child, Indentation + 1);
        }*/
    }
}
