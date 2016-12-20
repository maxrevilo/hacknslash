using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace BeautifyEffect
{
	public class Demo1 : MonoBehaviour
	{
		void Update ()
		{
			if (Input.GetKeyDown(KeyCode.T)) {
				Beautify.instance.enabled = !Beautify.instance.enabled;
				UpdateText();
			}
		}

		void UpdateText() {
			if (Beautify.instance.enabled) {
				GameObject.Find ("Beautify").GetComponent<Text>().text = "Beautify ON";
			} else {
				GameObject.Find ("Beautify").GetComponent<Text>().text = "Beautify OFF";
			}
		}
	}
}
