using System;

namespace ReflectionToIL.Benchmarking
{
    public static class Delegates
    {
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
