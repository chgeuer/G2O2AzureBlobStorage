namespace G2O
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using SignAlgorithm = System.Func<byte[], byte[], byte[], byte[]>;

    public static class G2OAlgorithms
    {
        public static string ComputeSignatureValue(int version, string key, string data, string urlpath)
        {
            if (version < 1 || 5 < version) { throw new ArgumentException(string.Format("Cannot find signature algorithm for version {0}", version)); }

            byte[] keyBytes = key.EncodedAsASCII();
            byte[] dataBytes = data.EncodedAsASCII();
            byte[] urlpathBytes = urlpath.EncodedAsASCII();

            SignAlgorithm algorithm = algorithms[version];
            byte[] signatureBytes = algorithm(keyBytes, dataBytes, urlpathBytes);

            string signature = Convert.ToBase64String(signatureBytes);
            return signature;
        }

        private static readonly ReadOnlyDictionary<int, SignAlgorithm> algorithms = new ReadOnlyDictionary<int, SignAlgorithm>(new Dictionary<int, SignAlgorithm>
        {
            //{ 1, (key, data, urlpath) => MD5(msg: new[] { key, data, urlpath }) },
            //{ 2, (key, data, urlpath) => MD5(msg: new[] { key, MD5(new[] { key, data, urlpath }) }) },
            //{ 3, (key, data, urlpath) => HMAC_Sign(alg: "HMACMD5", key: key, msg: new[] { data, urlpath }) },
            //{ 4, (key, data, urlpath) => HMAC_Sign(alg: "HMACSHA1", key: key, msg: new[] { data, urlpath }) },
            //{ 5, (key, data, urlpath) => HMAC_Sign(alg: "HMACSHA256", key: key, msg: new[] { data, urlpath }) }

            { 1, (key, data, urlpath) => MD5(key, data, urlpath) },
            { 2, (key, data, urlpath) => MD5(key, MD5(key, data, urlpath)) },
            { 3, (key, data, urlpath) => HMAC_Sign("HMACMD5", key, data, urlpath) },
            { 4, (key, data, urlpath) => HMAC_Sign("HMACSHA1", key, data, urlpath) },
            { 5, (key, data, urlpath) => HMAC_Sign("HMACSHA256", key, data, urlpath) }
        });

        private static byte[] EncodedAsASCII(this string str)
        {
            return global::System.Text.Encoding.ASCII.GetBytes(str);
        }

        private static byte[] HMAC_Sign(string alg, byte[] key, params byte[][] msg)
        {
            var f = global::System.Security.Cryptography.HMAC.Create(alg);
            f.Key = key;
            return f.ComputeHash(Concat(msg));
        }

        private static byte[] MD5(params byte[][] msg)
        {
            var f = System.Security.Cryptography.HashAlgorithm.Create("MD5");
            return f.ComputeHash(Concat(msg));
        }

        private static byte[] Concat(params byte[][] m)
        {
            byte[] rv = new byte[m.Sum(a => a.Length)];
            int offset = 0;
            foreach (byte[] array in m)
            {
                System.Buffer.BlockCopy(array, 0, rv, offset, array.Length);
                offset += array.Length;
            }
            return rv;
        }
    }
}