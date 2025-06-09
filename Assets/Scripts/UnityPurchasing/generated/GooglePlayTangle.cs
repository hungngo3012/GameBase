// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("FJeZlqYUl5yUFJeXljySCuUKv+Qz4xbgYvnKlG77tC0oldglnIGpX6YUl7Smm5CfvBDeEGGbl5eXk5aVJaN3Eymv3Ov2MReDE4yoxojCiODp3gTsLxD5TqdfrAz2gj6be56JIZ8Rlo07qKDd7/7eDJStsKz8T2o5tKFYS0TOFbC8NShBN6qMBnrXMoRY2i7FGCEEjF3aA841oci5cNCZAL+avkYTrjq2A4Doz6oJVTfjlfpSqaYyQseQFKOcdJSHAS2PWkTTSOXtmTt+lJiLwR3pR6jwtBx5DpIgpO58goxH52yrUIZufDSxdyOoB4Qn63zomX8EzlcHngQit4DmUGMLoFZIO8a3HVn75IwYQDkv9x7ZEEE2Xg5Wi1IOwKzYi5SVl5aX");
        private static int[] order = new int[] { 1,10,10,6,6,5,10,7,10,12,10,11,12,13,14 };
        private static int key = 150;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
