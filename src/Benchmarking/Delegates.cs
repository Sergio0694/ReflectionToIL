using System;

namespace ReflectionToIL.Benchmarking
{
    /// <summary>
    /// A <see langword="class"/> that exposes a series of delegates with associated closures
    /// </summary>
    public static class Delegates
    {
        /// <summary>
        /// Gets a <see cref="Delegate"/> with an associated closure with just a few fields
        /// </summary>
        public static Delegate Small
        {
            get
            {
                int[]
                    array1 = Array.Empty<int>(),
                    array2 = Array.Empty<int>();
                int value1 = 0, value2 = 0, value3 = 0;

                return new Action(() =>
                {
                    array1[value1] = value3;
                    array2[value2] = value3;
                });
            }
        }

        /// <summary>
        /// Gets a <see cref="Delegate"/> with a closure spanning two scopes, and 8 captured variables
        /// </summary>
        public static Delegate Medium
        {
            get
            {
                int[]
                    array1 = Array.Empty<int>(),
                    array2 = Array.Empty<int>();
                int value1 = 1, value2 = 2, value3 = 3;
                {
                    int[] array3 = Array.Empty<int>();
                    int value4 = 4, value5 = 5;

                    return new Action(() =>
                    {
                        int sum = value4 + value5;
                        array1[value1] = sum;
                        array2[value2] = sum;
                        array3[value3] = sum;
                    });
                }
            }
        }

        /// <summary>
        /// Gets a <see cref="Delegate"/> with a closure spanning three scopes, and 14 captured variables
        /// </summary>
        public static Delegate Large
        {
            get
            {
                int[]
                    array1 = Array.Empty<int>(),
                    array2 = Array.Empty<int>();
                int value1 = 1, value2 = 2, value3 = 3;
                {
                    int[] array3 = Array.Empty<int>();
                    int value4 = 4, value5 = 5;
                    {
                        int[]
                            array4 = Array.Empty<int>(),
                            array5 = Array.Empty<int>(),
                            array6 = Array.Empty<int>();
                        int value6 = 6, value7 = 7, value8 = 8;

                        return new Action(() =>
                        {
                            int sum = value7 + value8;
                            array1[value1] = sum;
                            array2[value2] = sum;
                            array3[value3] = sum;
                            array4[value4] = sum;
                            array5[value5] = sum;
                            array6[value6] = sum;
                        });
                    }
                }
            }
        }
    }
}
