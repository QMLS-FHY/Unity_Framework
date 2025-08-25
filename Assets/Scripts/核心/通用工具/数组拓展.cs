using System;
using System.Collections.Generic;

namespace 工具
{
    public static class 数组拓展
    {
        /// <summary>
        /// 查找满足条件(相等）的单个元素(有多个时返回第一个）
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="array">数组</param>
        /// <param name="condition">比较方法（委托）</param>
        /// <returns>返回目标对象</returns>
        public static T 查找<T>(this T[] array, Func<T, bool> condition)
        {
            for (int i = 0; i < array.Length; i++)
            {
                if (condition(array[i]))
                {
                    return array[i];
                }
            }
            return default(T);
        }

        /// <summary>
        /// 查找满足条件(相等）的多个元素
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="array">数组</param>
        /// <param name="condition">比较方法（委托）</param>
        /// <returns>返回目标对象</returns>
        public static T[] 筛选<T>(this T[] array, Func<T, bool> condition)
        {
            List<T> list = new List<T>();
            for (int i = 0; i < array.Length; i++)
            {
                if (condition(array[i]))
                {
                    list.Add(array[i]);
                }
            }
            return list.ToArray();
        }

        /// <summary>
        /// 查找数组中满足条件的最大值
        /// </summary>
        /// <typeparam name="T">数组类型</typeparam>
        /// <typeparam name="Q">比较依据的数据类型</typeparam>
        /// <param name="array">数组</param>
        /// <param name="condition">比较依据方法</param>
        /// <returns></returns>
        public static T 最大值<T, Q>(this T[] array, Func<T, Q> condition) where Q : IComparable
        {
            //若数组为空则返回默认（null)
            if (array.Length == 0) return default(T);
            T maxT = array[0];
            for (int i = 0; i < array.Length; i++)
            {
                if (condition(array[i]).CompareTo(condition(maxT)) > 0)
                    maxT = array[i];
            }
            return maxT;
        }


        /// <summary>
        /// 查找数组中满足条件的最小值
        /// </summary>
        /// <typeparam name="T">数组类型</typeparam>
        /// <typeparam name="Q">比较依据的数据类型</typeparam>
        /// <param name="array">数组</param>
        /// <param name="condition">比较依据方法</param>
        /// <returns></returns>
        public static T 最小值<T, Q>(this T[] array, Func<T, Q> condition) where Q : IComparable
        {
            //若数组为空则返回默认（null)
            if (array.Length == 0) return default(T);
            T minT = array[0];
            for (int i = 0; i < array.Length; i++)
            {
                if (condition(array[i]).CompareTo(condition(minT)) < 0)
                    minT = array[i];
            }
            return minT;
        }


        /// <summary>
        /// 数组升序排序
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <typeparam name="Q">比较依据</typeparam>
        /// <param name="array">待排序数组</param>
        /// <param name="condition">排序依据方法</param>
        public static void 升序<T, Q>(this T[] array, Func<T, Q> condition) where Q : IComparable
        {
            //冒泡排序
            for (int i = 0; i < array.Length - 1; i++)
            {
                for (int j = i + 1; j < array.Length; j++)
                {
                    if (condition(array[j]).CompareTo(condition(array[i])) > 0)
                    {
                        T temp = array[i];
                        array[j] = array[j];
                        array[i] = array[j];
                    }
                }
            }
        }


        /// <summary>
        /// 数组降序排序
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <typeparam name="Q">比较依据</typeparam>
        /// <param name="array">待排序数组</param>
        /// <param name="condition">排序依据方法</param>
        public static void 降序<T, Q>(this T[] array, Func<T, Q> condition) where Q : IComparable
        {
            //冒泡排序
            for (int i = 0; i < array.Length - 1; i++)
            {
                for (int j = i + 1; j < array.Length; j++)
                {
                    if (condition(array[j]).CompareTo(condition(array[i])) < 0)
                    {
                        T temp = array[i];
                        array[j] = array[j];
                        array[i] = array[j];
                    }
                }
            }
        }


        /// <summary>
        /// 在每个T中选出Q 返回Q[]  ---例如在所有敌人物体中获得所有敌人的动画脚本
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <typeparam name="Q">需要获取的类型</typeparam>
        /// <param name="array">原数组</param>
        /// <param name="condition">返回需要获取的类型</param>
        /// <returns>获取到类型的数组</returns>
        public static Q[] 获取所有<T, Q>(this T[] array, Func<T, Q> condition)
        {
            Q[] res = new Q[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                res[i] = condition(array[i]);
            }
            return res;
        }
    }
}