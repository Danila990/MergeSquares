using Cinemachine;
using UnityEngine;

namespace Core.Camera
{
    public class CameraRtView : MonoBehaviour
    {
        [SerializeField] private CinemachineVirtualCamera followCamera;
        [SerializeField] private Transform followTarget;
        [SerializeField] private UnityEngine.Camera mainCamera;

        private CinemachineVirtualCamera _activeCamera;
        private Transform _cameraFollow;
        private Transform _cameraLookAt;

        private Vector2 _previousPosition;
        private Vector2 _previousFollowPosition;
        private Vector2 _followPosition;

        private void Awake()
        {
            _activeCamera = followCamera;
            ChangeFollowTarget(followTarget);
        }

        private void ChangeFollowTarget(Transform followTransform)
        {
            if(followTransform != null)
            {
                _cameraFollow = followTransform;
                _cameraLookAt = followTransform;
                _activeCamera.Follow = _cameraFollow;
                _activeCamera.LookAt = _cameraLookAt;
            }
        }

        public Vector3 ScreenToWorld(Vector3 point) => mainCamera.ScreenToWorldPoint(point);
    }
}