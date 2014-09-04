using UnityEngine;
using System.Collections.Generic;
using Leap;

class Point {
	public Vector3 p;
	public Point next;
}

public class LinesGR : MonoBehaviour {

	Controller m_TempController;

	public Shader shader;

	private Mesh ml;
	private Material lmat;
	
	private Mesh ms;
	private Material smat;
	
	private Vector3 s;

	private float lineSize = 0.03f;
	
	private GUIStyle labelStyle;
	private GUIStyle linkStyle;
	
	private Point first;
	
	private float speed = 5.0f;

	private bool mic = false;


	void Start () {
		m_TempController = new Controller();

		labelStyle = new GUIStyle();
		labelStyle.normal.textColor = Color.black;
		
		linkStyle = new GUIStyle();
		linkStyle.normal.textColor = Color.blue;
		
		ml = new Mesh();
		lmat = new Material(shader);
		lmat.color = new Color(1,1,1,0.1f);
		
		ms = new Mesh();
		smat = new Material(shader);
		//smat.color = new Color(0.51f,0.82f,0.2f,0.1f); // lighter Leap green
		smat.color = new Color(0.36f,0.67f,0,0.1f);  // darker Leap green

	}

	void Update() {
		Frame f = m_TempController.Frame();
		// distance between thumb joint 1 and index joint 3
		if(FingerTriggerOn(f)) {

//		if(f.Hands.Count > 0 && f.Hands[0].PinchStrength > 0.3f) {

			Vector3 e = GetNewPoint();
			
			if(first == null) {
				first = new Point();
				first.p = transform.InverseTransformPoint(e);
			}
			
			if(s != Vector3.zero) {
				Vector3 ls = transform.TransformPoint(s);
				AddLine(ml, MakeQuad(ls, e, lineSize), false);
				
				Point points = first;
				while(points.next != null) {
					Vector3 next = transform.TransformPoint(points.p);
					float d = Vector3.Distance(next, ls);
					if(d < 1 && Random.value > 0.9f) {
						AddLine(ms, MakeQuad(next, ls, lineSize), false);
					}
					points = points.next;
				}
				
				Point np = new Point();
				np.p = transform.InverseTransformPoint(e);
				points.next = np;

			}
			
			s = transform.InverseTransformPoint(e);
		} else {
			s = Vector3.zero;
		}
		
		Draw();
		processInput();
	}

	bool FingerTriggerOn(Frame f) {

		// distance
		if(f.Hands.Count > 0) {

			//Debug.Log(Vector3.Dot(f.Hands[0].Fingers[0].Direction.ToUnity().normalized, f.Hands[0].Fingers[1].Direction.ToUnity().normalized));

			Vector a = f.Hands[0].Fingers[0].JointPosition(Finger.FingerJoint.JOINT_DIP);
			Vector b = f.Hands[0].Fingers[1].JointPosition(Finger.FingerJoint.JOINT_PIP);
			
			Vector3 a3 = new Vector3(a.x, a.y, a.z);
			Vector3 b3 = new Vector3(b.x, b.y, b.z);
			
			float distance = Vector3.Distance(a3,b3);
			Debug.Log (distance);
			return (distance < 50);
		}
		return false;
	}
	
	void Draw() {
		if(mic) {
			var howLoud = GameObject.Find("Audio object").GetComponent<MicrophoneInput>().loudness;
			
			lineSize = howLoud/50;
			if(lineSize <= 0) lineSize = 0.03f;
		}

		Graphics.DrawMesh(ml, transform.localToWorldMatrix, lmat, 0);
		Graphics.DrawMesh(ms, transform.localToWorldMatrix, smat, 0);
	}
	
	Vector3[] MakeQuad(Vector3 s, Vector3 e, float w) {
		w = w / 2;
		Vector3[] q = new Vector3[4];

		Vector3 n = Vector3.Cross(s, e);
		Vector3 l = Vector3.Cross(n, e-s);
		l.Normalize();
		
		q[0] = transform.InverseTransformPoint(s + l * w);
		q[1] = transform.InverseTransformPoint(s + l * -w);
		q[2] = transform.InverseTransformPoint(e + l * w);
		q[3] = transform.InverseTransformPoint(e + l * -w);

		return q;
	}
	
	void AddLine(Mesh m, Vector3[] quad, bool tmp) {
		int vl = m.vertices.Length;
		
		Vector3[] vs = m.vertices;
		if(!tmp || vl == 0) vs = resizeVertices(vs, 4);
		else vl -= 4;
		
		vs[vl] = quad[0];
		vs[vl+1] = quad[1];
		vs[vl+2] = quad[2];
		vs[vl+3] = quad[3];
		
		int tl = m.triangles.Length;
		
		int[] ts = m.triangles;
		if(!tmp || tl == 0) ts = resizeTraingles(ts, 6);
		else tl -= 6;
		ts[tl] = vl;
		ts[tl+1] = vl+1;
		ts[tl+2] = vl+2;
		ts[tl+3] = vl+1;
		ts[tl+4] = vl+3;
		ts[tl+5] = vl+2;
		
		m.vertices = vs;
		m.triangles = ts;
		m.RecalculateBounds();

	}
	
	void processInput() {
		float s = speed * Time.deltaTime;
		if(Input.GetKey(KeyCode.RightShift) || Input.GetKey(KeyCode.LeftShift)) s = s * 10;
		if(Input.GetKey(KeyCode.UpArrow)) transform.Rotate(-s, 0, 0);
		if(Input.GetKey(KeyCode.DownArrow)) transform.Rotate(s, 0, 0);
		if(Input.GetKey(KeyCode.LeftArrow)) transform.Rotate(0, -s, 0);
		if(Input.GetKey(KeyCode.RightArrow)) transform.Rotate(0, s, 0);
		
		if(Input.GetKeyDown(KeyCode.C)) {
			ml = new Mesh();
			ms = new Mesh();
			transform.rotation = Quaternion.identity;
			first = null;
		}
		if(Input.GetKeyDown(KeyCode.V)) {
			// toggle volume-controlled line width
			if(mic) mic = false;
			else mic = true;
		}
	}
	
	Vector3 GetNewPoint() {
		Frame f = m_TempController.Frame();
		if(f.Hands.Count > 0) {
			return GameObject.Find("special finger").transform.position;
		}
		return new Vector3(0,0,0);
	}
	
	Vector3[] resizeVertices(Vector3[] ovs, int ns) {
		Vector3[] nvs = new Vector3[ovs.Length + ns];
		for(int i = 0; i < ovs.Length; i++) nvs[i] = ovs[i];
		return nvs;
	}
	
	int[] resizeTraingles(int[] ovs, int ns) {
		int[] nvs = new int[ovs.Length + ns];
		for(int i = 0; i < ovs.Length; i++) nvs[i] = ovs[i];
		return nvs;
	}
	
	void OnGUI() {
/*		GUI.Label (new Rect (10, 10, 300, 24), "GR. Cursor keys to rotate (fast with Shift)", labelStyle);
		int vc = ml.vertices.Length + ms.vertices.Length;
		GUI.Label (new Rect (10, 26, 300, 24), "Drawing " + vc + " vertices. 'C' to clear", labelStyle);
*/
	}

}







