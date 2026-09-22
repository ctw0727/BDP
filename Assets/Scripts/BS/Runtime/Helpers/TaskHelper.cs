using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace BS.Helpers
{
    public static class TaskHelper
    {
        public static UniTask RunSafely(Task task)
        {
            return LogTaskExceptions(task);
        }

        public static UniTask RunSafely(UniTask task)
        {
            return LogTaskExceptions(task);
        }

        static async UniTask LogTaskExceptions(Task task)
        {
            try
            {
                await task;
            }
            catch (Exception ex)
            {
                if (!(ex is OperationCanceledException))
                {
                    Debug.LogError(ex.ToString());
                }

                throw;
            }
        }

        static async UniTask LogTaskExceptions(UniTask task)
        {
            try
            {
                await task;
            }
            catch (Exception ex)
            {
                if (!(ex is OperationCanceledException))
                {
                    Debug.LogError(ex.ToString());
                }

                throw;
            }
        }
    }
}