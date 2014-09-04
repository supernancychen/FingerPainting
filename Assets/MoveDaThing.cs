using UnityEngine;
using System.Collections;
using Leap;

public class MoveDaThing : MonoBehaviour {
	Controller m_TempController;
	GameObject m_finger;
	GameObject m_hand;
	public Vector3 m_offset;

	// Use this for initialization
	void Start () {
		m_TempController = new Controller();
		m_finger = Instantiate(Resources.Load("Prefabs/DrawPoint")) as GameObject;	
		m_finger.transform.parent = GameObject.Find("Main Camera").transform;

		UnityEngine.Screen.lockCursor = true;
		UnityEngine.Screen.showCursor = false;
	}
	
	// Update is called once per frame
	void Update () {
		Frame f = m_TempController.Frame();

		if (f.Hands.Count > 0) {
			m_finger.transform.localPosition = f.Hands[0].Fingers[1].TipPosition.ToUnityScaled() + m_offset;

			m_finger.name = "special finger";
			m_finger.transform.forward = f.Hands[0].Fingers[1].Direction.ToUnity();
			m_finger.rigidbody.velocity = m_finger.transform.TransformPoint(f.Hands[0].Fingers[1].TipVelocity.ToUnityScaled());
		}
	}

}
