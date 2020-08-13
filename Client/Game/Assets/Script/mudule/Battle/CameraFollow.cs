using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour {
	//距离矢量
	public Vector3 distance = new Vector3(0, 15, -22);//x代表角度
	//相机
	public Camera camera;
	//偏移值
	public Vector3 offset = new Vector3(0, 8f, 0);
	//相机移动速度
	public float speed = 20f;
	//最大和最小距离
	public float minDistanceZ = -35f;
	public float maxDistanceZ = -10f;
	//距离变化速度
	public float zoomSpeed = 2f;

	// Use this for initialization
	void Start () {
		//默认为主相机
		camera = Camera.main;
		//相机初始位置
		Vector3 pos = transform.position;
		Vector3 forward = transform.forward;
		Vector3 initPos = pos - 30*forward + Vector3.up*10;
		camera.transform.position = initPos;
	}

	//调整距离
	void Zoom(){
		float axis = Input.GetAxis("Mouse ScrollWheel");
		distance.z += axis*zoomSpeed;
		distance.z = Mathf.Clamp(distance.z, minDistanceZ, maxDistanceZ);
	}

	//调整角度
	void Rotate(){
		if(!Input.GetMouseButton(1)){//右键
			return;
		}
		float axis = Input.GetAxis("Mouse X");
		distance.x += 2*axis;
		distance.x = Mathf.Clamp(distance.x, -20, 20);
	}

	//所有组件update之后发生
	void LateUpdate () {
		//坦克位置
		Vector3 pos = transform.position;
		//坦克方向
		Vector3 forward = transform.forward;
		Vector3 rigiht = transform.right;
		//相机目标位置
		Vector3 targetPos = pos;
		targetPos = pos + forward*distance.z + rigiht*distance.x;
		targetPos.y += distance.y;
		//相机位置
		Vector3 cameraPos = camera.transform.position;
		cameraPos = Vector3.MoveTowards(cameraPos, targetPos,Time.deltaTime*speed);
		camera.transform.position = cameraPos;
		//对准坦克
		Camera.main.transform.LookAt(pos + offset);
		//调整距离
		Zoom();
		//调整角度
		Rotate();
	}
}



