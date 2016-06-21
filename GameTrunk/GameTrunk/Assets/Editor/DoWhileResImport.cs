using UnityEngine;
using UnityEditor;
using System.IO;


public class DoWhileResImport : AssetPostprocessor
{

    void OnPreprocessTexture()
    {
        TextureImporter textureImporter = assetImporter as TextureImporter;
        if (textureImporter.assetPath.Contains("#M"))
        {
            return;
        }
        FileInfo info = new FileInfo(textureImporter.assetPath);
        if (!(info.Extension.ToLower().Equals(".png") || info.Extension.ToLower().Equals(".exr")))
        {
            Debug.LogError(textureImporter.assetPath + "--不是png,或者exr图片,不允许导入!请删除此文件");
            return;
        }

        if (textureImporter.assetPath.Contains("Assets/ArtRes/ui"))
        {
            string AtlasName = new DirectoryInfo(Path.GetDirectoryName(assetPath)).Name;
            textureImporter.textureType = TextureImporterType.Sprite;
            textureImporter.spriteImportMode = SpriteImportMode.Single;
            textureImporter.spritePackingTag = AtlasName.Split('#')[0];
            textureImporter.mipmapEnabled = false;
            //textureImporter.SetAllowsAlphaSplitting(true);
            //textureImporter.SetPlatformTextureSettings("Android", 2048, TextureImporterFormat.ETC2_RGBA8, 0, false);
            //textureImporter.SetPlatformTextureSettings("iPhone", 2048, TextureImporterFormat.PVRTC_RGBA4, 0, false);
            //textureImporter.SetPlatformTextureSettings("Standalone", 2048, TextureImporterFormat.DXT5, 0, false);
        }
        else
        {
            textureImporter.textureType = TextureImporterType.Advanced;
            textureImporter.mipmapEnabled = true;
            textureImporter.mipmapFilter = TextureImporterMipFilter.BoxFilter;
            textureImporter.generateMipsInLinearSpace = true;
            textureImporter.borderMipmap = true;
            textureImporter.fadeout = false;
            textureImporter.filterMode = FilterMode.Bilinear;
            textureImporter.mipmapFilter = 0;
        }
        if (textureImporter.assetPath.Contains("#A"))
        {
            textureImporter.SetPlatformTextureSettings("Android", 2048, TextureImporterFormat.ETC2_RGBA8, 0, false);
            textureImporter.SetPlatformTextureSettings("iPhone", 2048, TextureImporterFormat.PVRTC_RGBA4, 0, false);
            textureImporter.SetPlatformTextureSettings("Standalone", 2048, TextureImporterFormat.DXT5, 0, false);
            
        }
        else
        {
            textureImporter.SetPlatformTextureSettings("Android", 2048, TextureImporterFormat.ETC_RGB4, 0, false);
            textureImporter.SetPlatformTextureSettings("iPhone", 2048, TextureImporterFormat.PVRTC_RGB4, 0, false);
            textureImporter.SetPlatformTextureSettings("Standalone", 2048, TextureImporterFormat.DXT1, 0, false);
        }

        textureImporter.isReadable = false;
        textureImporter.alphaIsTransparency = false;
    }
    void OnPostprocessTexture(Texture2D texture)
    {
        //TextureImporter textureImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        //textureImporter.alphaIsTransparency = false;
        // 从图片中剔除Alpha
        // 只能够在Post中调用
        StripeTextureAlpha.StripAlpha(texture, assetPath);
    }
    void OnPreprocessModel()
    {
        ModelImporter MI = assetImporter as ModelImporter;
        MI.animationType = ModelImporterAnimationType.None;
        MI.isReadable = false;
        MI.importAnimation = false;
        MI.importBlendShapes = false;
        MI.optimizeMesh = true;
        MI.generateSecondaryUV = false;
        MI.importMaterials = false;
        MI.importNormals = ModelImporterNormals.Import;
        MI.importTangents = ModelImporterTangents.None;
        if (MI.assetPath.Contains("#T"))
        {
            //生成副法线
            MI.importTangents = ModelImporterTangents.CalculateMikk;
        }
        if(MI.assetPath.Contains("#L"))
        {
            //生成第二层UV(光照贴图用)
            MI.generateSecondaryUV = true;
        }
        if(MI.assetPath.Contains("#B"))
        {
            //绑定骨骼的bind文件
            MI.animationType = ModelImporterAnimationType.Generic;
        }
        if (MI.assetPath.Contains("@"))
        {
            //动画
            MI.animationType = ModelImporterAnimationType.Generic;
            MI.optimizeMesh = false;
            MI.importAnimation = true;
            MI.importBlendShapes = true;
            MI.importNormals = ModelImporterNormals.None;
        }
        if (MI.assetPath.Contains("#O"))
        {
            //模型带动画
            MI.animationType = ModelImporterAnimationType.Generic;
            MI.optimizeMesh = true;
            MI.importAnimation = true;
            MI.importBlendShapes = true;
            MI.importNormals = ModelImporterNormals.Import;
        }
        if (MI.assetPath.Contains("#PS"))
        {
            //粒子需要喷射的网格,并且网格是被分离的,等待unity修复此bug再去掉
            MI.isReadable = true;
        }

        //else if (MI.assetPath.Contains("_o"))
        //{//静态非lightmap建筑物体
        //    MI.animationType = ModelImporterAnimationType.None;
        //    MI.importAnimation = false;
        //    MI.importBlendShapes = false;
        //    MI.optimizeMesh = true;
        //    MI.generateSecondaryUV = false;
        //    MI.importMaterials = true;
        //    MI.normalImportMode = ModelImporterTangentSpaceMode.Import;
        //    MI.tangentImportMode = ModelImporterTangentSpaceMode.None;

        //}
        //else
        //{
        //    MI.optimizeMesh = true;
        //    MI.generateSecondaryUV = false;
        //    MI.importMaterials = true;
        //    MI.normalImportMode = ModelImporterTangentSpaceMode.Import;
        //    MI.tangentImportMode = ModelImporterTangentSpaceMode.None;
        //}


    }
    private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromPath)
    {
        foreach (string move in movedAssets)
        {
            //这里重新 import一下
            AssetDatabase.ImportAsset(move,ImportAssetOptions.ForceSynchronousImport);
        }
    }

}
 