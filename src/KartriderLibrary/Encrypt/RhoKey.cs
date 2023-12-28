﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using KartLibrary.IO;
using System.Reflection;
using KartLibrary.File;

namespace KartLibrary.Encrypt
{
    public static class RhoKey
    {
        private static uint[,] EncryptVectors =
        {
            {
                0x299c0f16, 0xf78cf0c1, 0x28f7d164, 0x4f4a1241, 0xe96b0b81, 0xaffa5fc0, 0xf4ab1829, 0x14670a1a,
                0xe573f6c4, 0x23734f5f, 0xeccab115, 0xe34a8f0f, 0x97c81a72, 0xa3a73220, 0xe959ba, 0x1566b698,
                0x9dd895d3, 0x88e26b82, 0x26c1806e, 0x1c9cb08b, 0xcfcf5bf2, 0xc1cc4b74, 0x8b8d2de5, 0x623e1fe2,
                0x3917dd30, 0xdbbdc83, 0x4620be73, 0x4f86a571, 0x7a2b867d, 0x397031dc, 0x6d402ffb, 0x18eff601,
                0x1202ebaa, 0xa924e7d6, 0xe998c350, 0x935da2bd, 0x8f956321, 0xa8f2d501, 0x2b0d91cd, 0x16f8a765,
                0xc61fa968, 0x1650759e, 0xaac8488c, 0xca63bceb, 0xe8a8166e, 0x5d1e272e, 0x50a60a05, 0x21fc7894,
                0xa39a6386, 0xcdfab82b, 0x7442839c, 0xb75f7daf, 0xd7da67f8, 0x8a89dbb4, 0xbf311f0c, 0xa7c005ff,
                0x7d1a33af, 0x6ab3031d, 0x89e8978d, 0x570979e4, 0x63f8598, 0xf8c0526b, 0x81d00024, 0x680feeff,
                0xe54ee699, 0x8b8f7175, 0x38b21495, 0x3c875129, 0x697b8deb, 0xd82ecd92, 0x155ddac9, 0xb952aabb,
                0x96f011be, 0xc23ae454, 0xb9b9b1fb, 0xe9bb1cb0, 0xad6ec6db, 0xd29c3e5a, 0x5452c71c, 0x9d0c8e90,
                0xe3f5cb9b, 0xeec79ce9, 0x231d913, 0x4d0c6fd8, 0x1e5ae115, 0xa489d8c2, 0x5e6946fe, 0xe04f8c09,
                0x1d647870, 0x2e86e434, 0xa84b10f, 0x4f376c9d, 0xd7267b4e, 0x2bce8d72, 0x7052edb9, 0x4635a2be,
                0xbd288d3, 0xd820cefd, 0x4c78d785, 0x61d7955e, 0x33c6bce3, 0x52bb7ffe, 0x8bba913a, 0x9617f71b,
                0xe9725f5c, 0x5890583e, 0x77133792, 0x2fa0c5fa, 0xa6d5a259, 0xe7f6d013, 0xe3ad514a, 0x34e4f62e,
0x54b1e98d, 0x3642d0fb, 0x70a78f5e, 0x6a17c04a, 0x7865fd1, 0x4ced223d, 0x985d496c, 0x94c637f,
0x9c1a67b2, 0xe0818108, 0x4ee99b50, 0x7d45101a, 0x898aec8d, 0x78789962, 0x7d6b4386, 0x6f1cff43,
0x47e77476, 0x84af42a, 0x6fa1b3f8, 0x9b1f7369, 0xfecb6b75, 0x10f94fea, 0xc0c3c008, 0x672ce901,
0xd414841e, 0x2fc15312, 0x7b19bfad, 0xbdb50763, 0x3f138804, 0xa1392189, 0x2973f451, 0xf3029769,
0xfcb6c0ca, 0x432747c0, 0x2cd7dd5d, 0x3107aeb7, 0x2df1daf9, 0xc57ea737, 0x3c4f6348, 0x433dacff,
0x1d89e995, 0x9ae565c7, 0xf2ae62a9, 0xe58e2d0c, 0xbc94e862, 0x34072ad7, 0xd03d4f45, 0x46e8af00,
0x52657d15, 0x22345d3, 0x59064352, 0x78096d9e, 0x88710f94, 0xfc39ec9a, 0x9f8c9690, 0x29d12b70,
0x296939eb, 0xc9f97b84, 0x7bb55550, 0x59960aa0, 0xce0e6abf, 0xa9da1a85, 0x9fe254ac, 0xbc5f9326,
0xf8ed746a, 0xd5cb1112, 0x23883035, 0x8f8f580e, 0xc7009fca, 0xedcacf14, 0x3b9ade0a, 0xff52fc4,
0x9b5be455, 0x7bf399a4, 0x3b043e7f, 0xfb0ebebc, 0x1636b2da, 0x193f1925, 0xfab42730, 0x61d58e9e,
0x2f4f2fb3, 0x87b60127, 0x8a9b1c8, 0xbcb058ba, 0x85e3cd39, 0xd6801cf8, 0x5a504786, 0x2fbd6e6,
0x83f106f8, 0x7fb90ff7, 0xf39d4195, 0x1fea0335, 0xde97c505, 0x7a30673f, 0x2a6421ea, 0x70f64fc0,
0x8cdf9cb7, 0x44163b2b, 0x6a0f7b3, 0xd425da75, 0x20baa291, 0xfb8b5741, 0x6c6c0e80, 0x4767d562,
0xb186b673, 0xc7973ad7, 0x33e07c77, 0xd4ad7cc8, 0x62abfa9d, 0x40da72a9, 0x4ba01daa, 0x57537e0a,
0x8593cc95, 0x26af05a5, 0xaf40124e, 0x8f8f1e20, 0x122a876, 0xdb82f02a, 0x2fa5cbbe, 0x859b7e97,
0x51a23125, 0x7e1a9b30, 0x77f0ea2f, 0xdbf65383, 0x6b2ef297, 0xe57c6548, 0x43562c86, 0xe45f0754,
0x690ca5f0, 0x6850d03b, 0x7ad6f9e5, 0x8a4e22cf, 0x7b293071, 0x98f301e9, 0x73309df5, 0xdd362472,
0x9a596de7, 0x3e7c45e8, 0x588d9dba, 0xc6cb31bc, 0x32d52c40, 0x2824f388, 0x22be3f7d, 0xac0d5b50,
},
{
0xf93ea054, 0x2d389b0, 0x68c1001e, 0xe387d52d, 0x5d0a1112, 0xbb4eacac, 0x9581c28f, 0xb0df6c5a,
0x52a04bb3, 0xe22a1304, 0xcc633613, 0x5b7f8b, 0xea00cb2b, 0x93d7a7ec, 0x876c0e10, 0xbb8b9bbc,
0x145a3723, 0x41d12e83, 0x8aa53c8e, 0xa4ed10c6, 0x56093885, 0x86fa5fbe, 0x348992ec, 0xbc8df1f3,
0x97da66a3, 0x8331c392, 0x1e244d2f, 0xe4419d98, 0x27bffcfd, 0x83ad7d71, 0x1a125fe3, 0x13f2f8de,
0x267838e1, 0x8a73ba0f, 0xd1fdb0a1, 0xf76a844b, 0x5b87ff03, 0xafde8aae, 0x3138d0dc, 0x179d77bb,
0x47a408a6, 0xbcaf27fd, 0x7346fc55, 0xb3be3605, 0xa57e3bab, 0x56162746, 0x6fbac493, 0x2bc7ea55,
0xa5905e8d, 0xfab4f9fd, 0x430b5b26, 0xf1658a60, 0xf41f5738, 0x2c63da37, 0x8230f296, 0x25c8b3eb,
0x8ba68724, 0x8722aedf, 0xff6a35e8, 0xbcd61c36, 0xbbe991a0, 0x992b786b, 0x876e3323, 0xe5d9c6e9,
0xceee4d58, 0xaeba18ca, 0x3b8cd6f2, 0xae2e09cb, 0x40be7e7c, 0x1ddafe2, 0x708eed9, 0x19aef205,
0x706b11c9, 0x83d28dca, 0xb725e1f6, 0x6b075db1, 0x476f3041, 0x6ee5916e, 0x81ac5707, 0xecdc023d,
0xc02508a9, 0x1c38491f, 0xa4f890b5, 0xc192fa6f, 0xdc8ce584, 0x7dba0115, 0x564946d3, 0xb87cde40,
0x76e46dec, 0xf1640bf7, 0x15a1a28b, 0xe0a1231d, 0xc52b2fa6, 0x628e63c6, 0xc2f7ad85, 0x87ca3268,
0xc30aaa9d, 0x15ed512e, 0x762486a2, 0xecebfa15, 0x4aa91dc9, 0x91cbe282, 0x1efad65e, 0xa50ed8c2,
0xb996c38, 0x7112fc62, 0xeb687fa7, 0x91b6fec7, 0x31022822, 0xa085197b, 0x283418b0, 0x644dde44,
0xf96e929a, 0x2edd8106, 0x7e624f0a, 0xaf301c80, 0x11e6a99e, 0x4c4c91f5, 0x71784116, 0x7f5e2b84,
0x686a64e3, 0xcda6fc8f, 0x531efd73, 0x7826fcd8, 0x36c2b369, 0xc2e94432, 0x4c73ed83, 0xe3122eac,
0xa86e4541, 0xa6da7573, 0xe3267288, 0x6525236c, 0x85cbd981, 0x696f9c97, 0xc194ece5, 0xfeae2bc1,
0xcd8858ea, 0xc7bd0642, 0x2932e66, 0x550fbd87, 0xe0f5fac1, 0xa7db9598, 0x4d576787, 0x71b05dad,
0x7c158c3b, 0x5fb9de5c, 0xdeaa00e4, 0x6e80c116, 0xd86fe7d7, 0x8af1ec4d, 0x23727dd5, 0x563ec536,
0x3296bf85, 0x9ce6e8e, 0x5abd8df0, 0x6b2eb394, 0x6a2f7d3a, 0xdb695d15, 0x513f7dd1, 0x46e99f35,
0x22bc3117, 0x1373830a, 0xac340ac2, 0xdfea38e2, 0x77995805, 0x71621248, 0x5b46cc1a, 0xe287ce56,
0x609f0c77, 0x3ce57a4c, 0x938c4322, 0x3149d935, 0xf4ad6e9a, 0xe4f0eeea, 0x9c80061e, 0x243318c1,
0x3f6b0c36, 0xe49b9ff5, 0x7b123383, 0x71f17e84, 0x9c97c9a3, 0xe9b6b0ef, 0xbab402ee, 0xa7a9e8f2,
0xe6235e8e, 0x2dd6111f, 0x91c9825f, 0x16ee3727, 0x87e6e3dc, 0x43531ac4, 0x66308b4f, 0xe3b699b9,
0xbf274af9, 0x9af12d5b, 0x31350d53, 0xe943da7b, 0xe826a2b9, 0xb338755a, 0x19eb5c48, 0x15b8581b,
0x9932300e, 0xa79183bf, 0x9c76e, 0xe6d6fcbc, 0xaa60a401, 0xa950bfee, 0xce06c008, 0xbf71bafb,
0x182451b6, 0xbdb7de7f, 0xfcfbf71a, 0xae338052, 0x541f5561, 0x58300dfa, 0xf8a3d0be, 0x525143b,
0xca3bff04, 0x8289b331, 0x406b2680, 0xbe813072, 0xb5b6c06c, 0xf4f6ada9, 0x3d1f227a, 0x6c2a9bd7,
0x9476fc7b, 0xaac25926, 0x455f7815, 0x5f160f3d, 0xcb2701bb, 0x24adb6e5, 0xa4e7d1e8, 0xbb4a0def,
0xb856f12f, 0xfabb2d39, 0x1b818565, 0xd4109ff2, 0xa995288c, 0x12f786b6, 0x137907d1, 0x6e34f0ce,
0xe3891540, 0xf9a3d862, 0xc46413dd, 0x44fd7ef2, 0x14d1a5d, 0x7b478af4, 0xd426d55d, 0x325c8835,
0xe230288e, 0x7f83dfba, 0x7993bd1a, 0x3ce39d0b, 0xb0e5df62, 0xfd6ec053, 0x57080733, 0xdc73060d,
},
{
0x41cb999, 0x1f623e94, 0x96949187, 0xf2e29a04, 0xefacf35, 0x5312d771, 0x484e4a7f, 0x74ff3b1e,
0x7b93c47c, 0x9fe59cd5, 0x85417121, 0xffcbbc85, 0xc493a1c3, 0xf69b5ff4, 0x9fcaa4a3, 0x99a1986d,
0x9a1e0558, 0x174fc729, 0xaa840a6a, 0x464ec501, 0xfd5221bd, 0x1253baa4, 0x58bdfe78, 0x364b3639,
0xa42077a4, 0x95dcb52e, 0xbc697412, 0xb90c4158, 0x20bb8173, 0xe7eef328, 0x54151723, 0x7cc3682b,
0x8bab349c, 0x374c7aa4, 0xe40432b9, 0xd79722b7, 0xeb024e5f, 0xfb20956c, 0x6ca4996b, 0xbc0e37eb,
0xf2647240, 0x844d5150, 0x9d6b7999, 0x28212ee7, 0xcd1ff974, 0x1a3bf107, 0x7be50fee, 0xea548f88,
0x108cf045, 0xd90b65e9, 0x422f4476, 0xc637b4a9, 0x8ee40794, 0xed72652c, 0xde3a23dc, 0xefe69bee,
0x677f5742, 0x5f54ec36, 0x3a371b6a, 0x2cff9b14, 0x49337374, 0xba5c383f, 0x5af42a2, 0x31d5ffe3,
0xf872c572, 0x929c6612, 0x96bc11cd, 0x342f9821, 0xa28ec631, 0x6e77b468, 0xf0b18533, 0x14d4063c,
0x8e6e04e2, 0x1e902b7e, 0x37de6873, 0x5c048c2e, 0x90aaf404, 0x362f940b, 0xbbb0d5f7, 0x83257078,
0xcc9d3889, 0xb016ed79, 0xffe54160, 0x71954474, 0xd61044c4, 0xebb5ef9, 0x60cf6713, 0xbcc60523,
0xb2a481a4, 0xeec3b239, 0x7d5367b9, 0xa03c5534, 0x3764eb5d, 0x31f14534, 0x57467173, 0x243c450,
0x95f62318, 0x2f238442, 0x963d5d4, 0xe58295d4, 0x476fd77d, 0x5ec848a8, 0x751c5dce, 0x5a1e30f1,
0x3780f71a, 0xe7120142, 0xe7f2e782, 0x9be1507c, 0x97df584e, 0xa3532615, 0x369dc3d6, 0x7d8eee0b,
0x5fc9e0c7, 0xfcb0753b, 0xef199c51, 0xa038d8f7, 0x1290f523, 0x16a4a1ff, 0x7b55d712, 0xc8fedc4f,
0x83d9ece8, 0x4957ccd2, 0xc61ed3d7, 0x2453cd3f, 0x36d7f83, 0xe32b66c9, 0xa6d2a24, 0x421e110,
0x3d484a6f, 0x76a45a20, 0xa792dd4b, 0xfcdc75ce, 0x2cc329ae, 0x48ed4c4d, 0x69cba2f7, 0x6d8b012,
0x39731c47, 0x98c4353e, 0x83a197d8, 0x5067d1d7, 0x509d1eed, 0x61b08167, 0xed773d8d, 0xc341399e,
0xce5ac8b4, 0x5878c8fb, 0x4a45c9c5, 0x382ad267, 0x737cb15, 0x6f5e545c, 0xb5a897f6, 0x44b99756,
0xd30ee36c, 0x554c5485, 0xaa9790c6, 0x9c09e47a, 0x6788143e, 0x4aae9f51, 0x845cb32a, 0x66ab99a0,
0x8e3fdd9c, 0x4d6c34ff, 0xc9025681, 0x6a540544, 0xe561572a, 0x79091e83, 0xe13aafa1, 0x77e5230e,
0x6e73ad46, 0x7c0787ed, 0x1f0e6a55, 0xecaccb05, 0x9be99cc4, 0x763b9050, 0xbc5ffaad, 0x69a2dc2a,
0xc80e84d8, 0x177bec8c, 0x9817e71f, 0xd4495ddb, 0x42625cc4, 0x413fa0ba, 0x522aa31f, 0xf3250367,
0x2ddccfa9, 0xf1f57477, 0xae3e5003, 0x5fd2b25e, 0x3b097c33, 0xd3c70926, 0x1285448, 0x1a294137,
0x729bbe2d, 0x8dfe381f, 0x43788c2b, 0x91e41634, 0x8564f03e, 0xea24f9b9, 0x22c21922, 0xe145e12b,
0x485d3e2d, 0x8f5c76a4, 0x18334a51, 0xd8064075, 0x8f04ff82, 0x2ef64ffa, 0x195695d9, 0xf2fadf22,
0xb840b4c3, 0x194c3102, 0x1886a92, 0xd3bb6b28, 0x50f9030f, 0x8beef8ef, 0xb44ea1ec, 0x28114899,
0xac9d9e69, 0x468d4b3f, 0x1a28bf7c, 0x94fcff05, 0x26ffdc8a, 0x10296967, 0x7d34651a, 0x5ce7a17c,
0x61ae35ef, 0xa5dc393a, 0x320ab681, 0x9a321719, 0x5d78cd89, 0x44ba1222, 0xdc78620b, 0xb2d591b,
0x3c101f8f, 0xffc86d14, 0xf99377f8, 0xb412d135, 0x4f29e2c6, 0x3894839b, 0xdd2baf19, 0x47dacd55,
0x69500595, 0x9c2307e7, 0x2b087728, 0x980b7048, 0xee107332, 0x782bf767, 0x355df893, 0x3e8b3668,
0x2f32535a, 0x62879e7e, 0xcfa78cea, 0xe7178747, 0xc5d279f3, 0xd85fbc0b, 0xc1117aea, 0xc0a9daf0,
},
{
0x3b0ca8a7, 0xe3890626, 0xa5b9cc83, 0xd9727743, 0x63e8495c, 0x25ea644c, 0x5031ccae, 0xc8c004a4,
0xeb9e0ee5, 0x875a4458, 0x1571cae7, 0xa9eb22f5, 0xc5a5b80c, 0xcc056956, 0x21489bd8, 0xf6402cf3,
0xfe2219ca, 0xee8a10ab, 0x4e065877, 0xcb8b1c46, 0x50bd5b8b, 0xb8c676ac, 0x9f4a804e, 0x3970d10c,
0x2dbff0d, 0x45df52ff, 0x61c2fef0, 0x264ab180, 0xc470456f, 0x311c8c75, 0x896d3ce0, 0x325af5f4,
0x1409a470, 0x4c7a79df, 0xc6de6b4e, 0x82ab498d, 0x1e527576, 0x33f3bc2c, 0xfc6b969b, 0x1c888600,
0xdfa06e42, 0xac702db2, 0xc779fffe, 0x3de90d39, 0xed1d33e7, 0x9d27c7b2, 0x93625e8c, 0xa6137f2d,
0x297aaa77, 0xd834a15d, 0xc4e49f6, 0xbb0c70ba, 0xf0a899d, 0xc3ce18e9, 0x6a8385bd, 0xda72e514,
0x7cefb9eb, 0x9fc25b21, 0x771dd696, 0xe7e5ea5d, 0xb46c8874, 0x2903c5ce, 0xe46fd0f0, 0x771bc042,
0x7a4f8ea0, 0xa2dfe861, 0xeaab219, 0xcb0e1438, 0x9b9d7332, 0xc96b3123, 0xf45056af, 0x931a8f62,
0x822801a0, 0x9088f338, 0xa869c8b2, 0xb60a15ff, 0xf99f47ff, 0xbba66139, 0x6fe07af3, 0xad437a37,
0x71915551, 0xe0347ad8, 0x256e855d, 0x44956a2a, 0x86c92af0, 0x54e44f07, 0x701fb42a, 0x5a7d164f,
0x5c1a24d, 0xca7e3e12, 0xa069c760, 0x7e548a80, 0x6c560e6, 0x8708a0dc, 0x323d36f8, 0x1947515,
0xedd13a49, 0x30b0b56f, 0x37bf2a93, 0xaae35f40, 0x2a448c21, 0x191fc5dc, 0xb32fcaf, 0x9ee85933,
0x9a88a938, 0x2cb34b3e, 0xfad7f8f8, 0x1c5258bb, 0x955c83fb, 0xaa0dde99, 0xc10889dc, 0xbcced373,
0xfa562689, 0xacf50ff7, 0x19a86f96, 0x423905d6, 0xb79d92e6, 0x4551dc93, 0x60080555, 0x953217fd,
0x91e84729, 0x7e770ad8, 0xd52800fb, 0xdbe29d39, 0xbca04ab3, 0x937efb12, 0xc4d9e0d0, 0xee176535,
0xb3125116, 0xdec00360, 0x74f9fada, 0x5b5deb83, 0xdcbc9e7c, 0xcfd352bd, 0x8d85fd9d, 0xe8d1194f,
0xbc4bfa1d, 0x9543660f, 0x1771f1d1, 0x1bb7a859, 0xeb7c11db, 0x1cad32b0, 0x9644bc0a, 0x9215d27,
0xecd2a66c, 0x7ea97699, 0x3bff3cb4, 0xda0b1ba3, 0x803ed33f, 0x44f91bfd, 0x58d74d8a, 0x56bc9caa,
0xe583ba54, 0x11788db7, 0xbd69e8de, 0x7d77b914, 0x8dbe4f4c, 0x478630f9, 0x9029cff8, 0x8a409de6,
0x70338920, 0xec03c4e2, 0x555dc618, 0xd2b54750, 0x7ded729, 0xe60693f2, 0x637c608f, 0xaaa2749,
0xffe023d8, 0x1d97c0a8, 0x7eeff25e, 0x51d15f20, 0xa3ec9a13, 0xc3398357, 0xaad66104, 0x1b219461,
0xf773d8ff, 0x9d11abc8, 0x689a80d2, 0x8ee8e3bd, 0x89edaa80, 0x9e08e535, 0x81205c84, 0x184ea75c,
0x9572329e, 0x94f86fdf, 0x6d404012, 0xe37ea1f, 0x8e13bfb3, 0x82b31f54, 0xceb9e33d, 0x54cf1f71,
0xa870567e, 0xd401cd1b, 0x6539f5be, 0x102f68ad, 0xce4ec5ca, 0x11dedf9e, 0x9fdf6788, 0x984bb5f8,
0xcf1746f9, 0xe49fbcc4, 0x40f818e3, 0x907605a9, 0x32fd52df, 0x89a4cd64, 0x6f265313, 0xc279301a,
0x6ac21ba3, 0xd49fbd46, 0x430b4447, 0x2da2d201, 0xf383e35e, 0x95e1464b, 0x68b34881, 0xbcd6b0be,
0x101633da, 0x973bcc08, 0xfc2e071e, 0x9babf2fe, 0xb254444a, 0xf81597fd, 0xdbfd69b3, 0x42d189be,
0x87e4b856, 0xb6bc1962, 0xa001bd34, 0x926e86d, 0x76ef57c4, 0xc2ad8631, 0xdcae4e1, 0xb297fff0,
0x98781144, 0xf1fe69fa, 0xdae9b39a, 0x71091afe, 0x7e940bc8, 0x78b5cb12, 0x601ea5fa, 0xba6fda28,
0x8cec7ba9, 0xe89725e8, 0x587957d3, 0x9d3304fa, 0xb8fe960, 0xa4b40b06, 0xe23d7921, 0x5220dce0,
0x57eeec3, 0x452cc378, 0x84d25412, 0xd377f01e, 0xd118d463, 0xf57d7e65, 0xffc7c9ef, 0xc73b1758,
},

};

        public static uint GetRhoKey(string FileName)
        {
            byte[] stringData = Encoding.GetEncoding("UTF-16").GetBytes(FileName);
            return Adler.Adler32(0, stringData, 0, stringData.Length) - 0xa6ee7565;
        }

        public static uint GetJmdKey(string FileName)
        {
            byte[] stringData = Encoding.GetEncoding("UTF-16").GetBytes(FileName);
            return Adler.Adler32(0, stringData, 0, stringData.Length) + 0x3de90dc3;
        }

        public static uint GetBlockFirstKey(uint RhoKey)
        {
            return RhoKey ^ 0x3A9213AC;
        }



        public static uint GetDirectoryDataKey(uint RhoKey)
        {
            return RhoKey + 0x2593A9F1;
        }

        public static uint GetJmdDirectoryDataKey(uint RhoKey)
        {
            return RhoKey - 0x41014EBF;
        }

        public static uint GetDataKey(uint RhoKey,RhoFileInfo fileInfo)
        {
            byte[] strData = Encoding.GetEncoding("UTF-16").GetBytes(fileInfo.Name);
            uint key = Adler.Adler32(0, strData, 0, strData.Length);
            key += (uint)fileInfo.ExtNum;
            key += (RhoKey - 0x756DE654);
            return key;
        }

        public static uint GetFileKey(uint RhoKey, string fileName, uint extNum)
        {
            byte[] strData = Encoding.GetEncoding("UTF-16").GetBytes(fileName);
            uint key = Adler.Adler32(0, strData, 0, strData.Length);
            key += extNum;
            key += (RhoKey - 0x756DE654);
            return key;
        }

        public static uint GetJmdDataKey(uint RhoKey, RhoFileInfo fileInfo)
        {
            byte[] strData = Encoding.GetEncoding("UTF-16").GetBytes(fileInfo.Name);
            uint key = Adler.Adler32(0, strData, 0, strData.Length);
            key += (uint)fileInfo.ExtNum;
            key += (RhoKey - 0x7E2AF33D);
            return key;
        }

        public static unsafe byte[] ExtendKey(uint originalKey)
        {
            byte[] outArray = new byte[64];
            fixed(byte* wPtr = outArray)
            {
                uint *writePtr = (uint*)wPtr;
                uint curData = originalKey ^ 0x8473fbc1;
                for (int i = 0; i < 16; i++)
                {
                    writePtr[i] = curData;
                    curData -= 0x7b8c043f;
                }
            }
            return outArray;
        }

        public static uint GetVector(uint value)
        {
            uint output = 0;
            for (int i = 0; i < 4; i++)
            {
                output ^= EncryptVectors[i, (value >> (i << 3)) & 0xFF];
            }
            return output;
        }

        /*Rho5test func*/
        public static byte[] getKey(string filename,string anotherData)
        {
            filename = filename.ToLower();
            string newStr = $"{filename}{anotherData}";
            byte[] data = Encoding.GetEncoding("UTF-16").GetBytes(newStr);
            byte[] output = new byte[0x80];
            int readsCount = data.Length >> 1;
            for(int i = 0; i < 128; i++)
            {
                int index = i % readsCount;
                output[i] = (byte)(data[index*2] + i);
            }
            return output;
        }
    }
}
