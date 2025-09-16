using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Common
{
    /// <summary>
    /// 読み取り専用二次元配列
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IReadOnlyTwoDimensionArray<T>
    {
        T[] this[int row] { get; }
        T this[int row, int column] { get; }
        
        int RowCount { get; }
        int ColumnCount { get; }
        int AllCount { get; }
        
        T[][] ToArray();
    }
    
    /// <summary>
    /// シリアライズ可能な二次元配列(長方形)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class SerializableTwoDimensionArray<T>: IReadOnlyTwoDimensionArray<T>, IEnumerable<T>
    {
        [SerializeField]
        private Row[] _rows;

        /// <summary>
        /// 行だけ（array[1]）
        /// </summary>
        /// <param name="row"></param>
        public T[] this[int row]
        {
            get
            {
                return _rows[row].ToArray();
            }
            set
            {
                _rows[row] = new(value);
            }
        }

        /// <summary>
        /// 二次元（array[1, 2]）
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        public T this[int row, int col]
        {
            get => _rows[row][col];
            set => _rows[row][col] = value;
        }
        
        /// <summary>
        /// 行数
        /// </summary>
        public int RowCount => _rows.Length;
        /// <summary>
        /// 列数
        /// </summary>
        public int ColumnCount => _rows[0].Length;

        /// <summary>
        /// 全要素数
        /// </summary>
        public int AllCount => RowCount*ColumnCount;

        public SerializableTwoDimensionArray(int rowCount, int columnCount)
        {
            _rows = new Row[rowCount];

            for (int i = 0; i < rowCount; i++)
            {
                _rows[i] = new Row(columnCount);
            }
        }
        
        /// <summary>
        /// 二次元配列に変換(内部の配列をコピーして返す)
        /// </summary>
        public T[][] ToArray()
        {
            int rowLength = _rows.Length;
            var array = new T[rowLength][];

            for (int i = 0; i < rowLength; i++)
            {
                array[i] = _rows[i].ToArray();
            }

            return array;
        }
        
        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < RowCount; i++)
            {
                for (int j = 0; j < ColumnCount; j++)
                {
                    yield return this[i, j];
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// 一行
        /// </summary>
        [Serializable]
        private class Row
        {
            [SerializeField]
            private T[] _content;
            
            /// <summary>
            /// 要素数
            /// </summary>
            public int Length => _content.Length;

            public Row(int length)
            {
                _content = new T[length];
            }

            public Row(params T[] content)
            {
                _content = Copy(content.Length, content);
            }

            public Row(Row row)
            {
                _content = row.ToArray();
            }

            public T this[int rowIndex]
            {
                get => _content[rowIndex];
                set => _content[rowIndex] = value;
            }

            /// <summary>
            /// 配列に変換(内部の配列をコピーして返す)
            /// </summary>
            /// <returns></returns>
            public T[] ToArray()
            {
                return Copy(Length, _content);
            }

            /// <summary>
            /// 配列をコピー
            /// </summary>
            /// <param name="length"></param>
            /// <param name="sourceArray"></param>
            /// <returns></returns>
            private T[] Copy(int length, T[] sourceArray)
            {
                var destinationArray = new T[length];
                Array.Copy(sourceArray, destinationArray, sourceArray.Length);
                return destinationArray;
            }
        }
    }
}