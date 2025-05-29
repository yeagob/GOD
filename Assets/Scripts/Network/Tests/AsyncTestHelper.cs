using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;

namespace Network.Tests
{
    /// <summary>
    /// Utilidad para ayudar a probar el código asíncrono basado en callbacks en tests de Unity
    /// </summary>
    /// <summary>
    /// Clase para mantener el estado de operaciones asíncronas
    /// </summary>
    /// <typeparam name="T">Tipo del resultado</typeparam>
    public class AsyncOperationState<T>
    {
        public bool IsDone { get; private set; }
        public T Result { get; private set; }
        
        public void SetDone(T result)
        {
            Result = result;
            IsDone = true;
        }
        
        public void Reset()
        {
            IsDone = false;
            Result = default;
        }
        
        public Action<T> CreateCallback()
        {
            return (T value) => SetDone(value);
        }
    }
    
    public static class AsyncTestHelper
    {
        /// <summary>
        /// Crea una corrutina para esperar hasta que se cumpla una condición
        /// </summary>
        /// <param name="condition">Condición a evaluar</param>
        /// <param name="timeoutSeconds">Tiempo máximo de espera en segundos</param>
        /// <returns>Una corrutina que espera hasta que la condición sea verdadera o se agote el tiempo</returns>
        public static IEnumerator WaitUntil(Func<bool> condition, float timeoutSeconds = 1.0f)
        {
            float startTime = Time.time;
            
            while (!condition())
            {
                if (Time.time - startTime > timeoutSeconds)
                {
                    Debug.LogError("Timeout esperando que se cumpla la condición");
                    yield break;
                }
                
                yield return null;
            }
        }
        
        /// <summary>
        /// Crea una corrutina para ejecutar una operación asíncrona y esperar a que se complete
        /// </summary>
        /// <typeparam name="T">Tipo del resultado</typeparam>
        /// <param name="operation">Operación a ejecutar</param>
        /// <param name="state">Estado de la operación que se actualizará cuando se complete</param>
        /// <param name="timeoutSeconds">Tiempo máximo de espera en segundos</param>
        /// <returns>Una corrutina que ejecuta la operación y espera su finalización</returns>
        public static IEnumerator ExecuteAsync<T>(Action<Action<T>> operation, AsyncOperationState<T> state, float timeoutSeconds = 1.0f)
        {
            state.Reset();
            operation(state.CreateCallback());
            
            yield return WaitUntil(() => state.IsDone, timeoutSeconds);
        }
        
        /// <summary>
        /// Overload para operaciones sin resultado (Action<Action<bool>> por ejemplo)
        /// </summary>
        public static IEnumerator ExecuteAsync(Action operation, AsyncOperationState<bool> state, float timeoutSeconds = 1.0f)
        {
            state.Reset();
            operation();
            
            yield return WaitUntil(() => state.IsDone, timeoutSeconds);
        }
    }
}