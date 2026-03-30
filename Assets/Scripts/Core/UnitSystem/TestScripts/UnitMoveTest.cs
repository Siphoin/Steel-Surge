using UnityEngine;
using SteelSurge.Core.UnitSystem;
using SteelSurge.Core.UnitSystem.Components;

namespace SteelSurge.Core.UnitSystem.TestScripts
{
    [RequireComponent(typeof(Unit))]
    public class UnitMoveTest : MonoBehaviour
    {
        private Unit _unit;
        private Camera _mainCamera;

        private void Awake()
        {
            _unit = GetComponent<Unit>();
            _mainCamera = Camera.main;
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    _unit.StateMachine.SetTargetPoint(hit.point);
                    _unit.NavMesh.SetDestination(hit.point);
                }
            }
        }
    }
}
