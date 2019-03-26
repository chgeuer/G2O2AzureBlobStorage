namespace G2OTests
{
    using G2O;
    using NUnit.Framework;

    public class G2OCryptoUnitTest
    {
        [TestCase(1, "1, 88.221.93.34, 91.52.154.64, 1425914540, 138158875.230879460, 193565", "HTjX4ex04yVDw45DxD/Wvg==", "/version1/portalvhdsdll14kvm02ts7/private/Bg5eMRyIIAAuAKC.jpg", "193565", "07bf84629be85d68a3ef343d")]
        [TestCase(2, "2, 80.157.170.6, 91.52.154.64, 1425914571, 406028399.967117982, 193565", "54C8dmt8PWAdbTmn7rtCug==", "/version2/portalvhdsdll14kvm02ts7/public/BgILp81CMAI_CN1.jpg", "193565", "07bf84629be85d68a3ef343d")]
        [TestCase(3, "3, 88.221.93.12, 91.52.154.64, 1425914629, 13587203.1988984900, 193565", "RghQKaP2+Am00Qo3b3OBpA==", "/version3/portalvhdsdll14kvm02ts7/private/Bg5eMRyIIAAuAKC.jpg", "193565", "07bf84629be85d68a3ef343d")]
        [TestCase(4, "4, 88.221.93.34, 91.52.154.64, 1425914666, 138246305.315416849, 193565", "yPhp1SKho6ChHYbJ2h28roruI2A=", "/version4/chgeuerwe123/public/BgsslfBCIAAjERn.jpg", "193565", "07bf84629be85d68a3ef343d")]
        [TestCase(5, "5, 88.221.93.12, 91.52.154.64, 1425914708, 13634961.563907161, 193565", "ywlg2wx5naj+stk1mJvz3zpcrkX5UNjt+0B04EwrlhU=", "/version5/portalvhdsdll14kvm02ts7/private/Bg5eMRyIIAAuAKC.jpg", "193565", "07bf84629be85d68a3ef343d")]
        [TestCase(10, "1, 88.221.93.29, 91.52.152.95, 1426003084, 87326004.774858761, 193565", "wRtlJxlo9IjAf3P0DO1UBA==", "/version1/chgeuerwe123/public/Fo3321", "193565", "07bf84629be85d68a3ef343d")]
        public void TestCaseFooBar(int version, string data, string signature, string path, string nonce, string nonceValue)
        {
            Assert.IsTrue(new G2OData.Signature(data, signature).Validate(path, _ignore => nonceValue), string.Format("version {0}", version));
        }
    }
}