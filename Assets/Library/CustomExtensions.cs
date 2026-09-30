using System;
using System.Linq;
using UnityEngine;

#region    ...
#endregion ...

namespace CustomExtension
{
    public static class ArrayExtensions
    {
        // Resize //
            public static void AddNewToArray<GenericArray>(ref GenericArray[] GenArr_A1_Source, GenericArray GenArr_Element)
            {
                // Create If Empty //
                    if (GenArr_A1_Source == null)
                    {
                        GenArr_A1_Source = new GenericArray[1];
                        GenArr_A1_Source[0] = GenArr_Element;
                        return;
                    } 
                // Remove //
                Array.Resize(ref GenArr_A1_Source, GenArr_A1_Source.Length + 1);
                GenArr_A1_Source[GenArr_A1_Source.Length - 1] = GenArr_Element;
            }
            public static void RemoveSpecificFromArray<GenericArray>(ref GenericArray[] GenArr_A1_Source, GenericArray GenArr_Remove)
            {
                // Exit If Empty //
                    if (GenArr_A1_Source == null) return;
                    if (GenArr_Remove == null)    {Debug.Log("ERROR: RemoveSpecificFromArray"); return;} 
                // Remove //
                    int Int_Index = Array.IndexOf(GenArr_A1_Source, GenArr_Remove);
                    if (Int_Index < 0) return;
                    RemoveByIndexFromArray(ref GenArr_A1_Source, Int_Index);
            }
            public static void RemoveByIndexFromArray<GenericArray>(ref GenericArray[] GenArr_A1_Source, int Int_Index)
            {
                // Exit If Empty //
                    if (GenArr_A1_Source == null) return;
                // Remove //
                    // Imposibble Case //
                        if (Int_Index < 0 || Int_Index >= GenArr_A1_Source.Length)
                            return;
                    // Variable //
                        GenericArray[] GenArr_A1_Temp = new GenericArray[GenArr_A1_Source.Length - 1];
                    // Before Section //
                        if (Int_Index > 0)
                            Array.Copy(GenArr_A1_Source, 0, GenArr_A1_Temp, 0, Int_Index);
                    // After Section //
                        if (Int_Index < GenArr_A1_Source.Length - 1)
                            Array.Copy(GenArr_A1_Source, Int_Index + 1, GenArr_A1_Temp, Int_Index, GenArr_A1_Source.Length - Int_Index - 1);
                    // Apply Changes //
                        GenArr_A1_Source = GenArr_A1_Temp;
            }
        // Compression/Decompression //
            public static void Flatten2DArray<GenericArray>(GenericArray[,] GenArr_A2_In, int Int_W, int Int_H, out GenericArray[] GenArr_A1_Out)
            {
                GenArr_A1_Out = new GenericArray[Int_W * Int_H];
                for (int Int_IndexY = 0; Int_IndexY < Int_H; Int_IndexY++)
                {
                    for (int Int_IndexX = 0; Int_IndexX < Int_W; Int_IndexX++)
                    {
                        GenArr_A1_Out[Int_IndexY*Int_W+Int_IndexX] = GenArr_A2_In[Int_IndexX, Int_IndexY];
                    }
                }
            }
            public static void UnFlatten1DArray<GenericArray>(GenericArray[] GenArr_A1_In, int Int_W, int Int_H, out GenericArray[,] GenArr_A2_Out)
            {
                GenArr_A2_Out = new GenericArray[Int_W, Int_H];
                for (int Int_IndexY = 0; Int_IndexY < Int_H; Int_IndexY++)
                {
                    for (int Int_IndexX = 0; Int_IndexX < Int_W; Int_IndexX++)
                    {
                        GenArr_A2_Out[Int_IndexX, Int_IndexY] = GenArr_A1_In[Int_IndexY*Int_W+Int_IndexX];
                    }
                }
            }
    }
}