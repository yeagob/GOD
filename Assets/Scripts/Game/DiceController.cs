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

        if (_diceRb.transform.forward == Vector3.forward)
            return 6;
        if (_diceRb.transform.forward == Vector3.back)
			return 1;
        if (_diceRb.transform.up == Vector3.forward)
			return 5;
        if (_diceRb.transform.up == Vector3.back)
			return 2;
        if (_diceRb.transform.right == Vector3.forward)
			return 3;
        if (_diceRb.transform.right == Vector3.back)
			return 4;


        return await RollDice();


	}

}
