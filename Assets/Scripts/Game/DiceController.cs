using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class DiceController : MonoBehaviour
{
    [SerializeField] private Vector3 _initialRollPosition;
    [SerializeField] private float _initialRollForce = 500;
    [SerializeField] private Rigidbody _diceRb;
    [SerializeField] private Camera _diceCamera;

    private Vector3[] _forwardDirections =
    {
        Vector3.back,//1
        Vector3.up,//2
        Vector3.left,//3
        Vector3.right,//4
        Vector3.zero,
        Vector3.zero,

    };

    private Vector3[] _upDirections =
    {
        Vector3.zero,
        Vector3.back,//2
        Vector3.left,//3
        Vector3.right,//4
        Vector3.forward,//5
        Vector3.zero,

    };

    private Vector3[] _rightDirections =
{
        Vector3.left,//1
        Vector3.zero,
        Vector3.forward,//3
        Vector3.back,//4
        Vector3.zero,
        Vector3.right,//6
	};

    // Start is called before the first frame update
    void Awake()
    {
		_diceCamera.gameObject.SetActive(false);
        _diceRb.transform.rotation = Quaternion.Euler(Random.insideUnitSphere);
    }

    // Update is called once per frame
    public async Task<int> RollDice()
    {
		_diceCamera.gameObject.SetActive(true);

        _diceRb.WakeUp();
        _diceRb.transform.localPosition = _initialRollPosition;
        _diceRb.velocity = Vector3.zero;
        _diceRb.AddTorque(Random.insideUnitSphere * _initialRollForce);

        float maxTime = 5f;

        while (!_diceRb.IsSleeping() && maxTime > 0)
		{
            maxTime -= Time.deltaTime;
            await Task.Yield();
        }

		_diceCamera.gameObject.SetActive(false);

        for (int i = 0; i < 6; i++)
		{
            if (_diceRb.transform.forward == _forwardDirections[i])
                return i + 1;
            if (_diceRb.transform.up == _upDirections[i])
                return i + 1;
            if (_diceRb.transform.right == _rightDirections[i])
                return i + 1;
		}

        //en caso de duda, jejeje
        return 6;

    }
}
