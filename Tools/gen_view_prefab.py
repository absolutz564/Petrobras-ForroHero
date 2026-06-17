"""
Gera prefabs de View do Forro Hero (View - QR e View - Obrigado),
modelados na estrutura do 'View - Results' (Canvas overlay + CanvasGroup +
GraphicRaycaster + script). Saida: .prefab (o .meta e escrito separado).
"""
import sys

# GUIDs de componentes/scripts (do projeto)
G_RAYCASTER = "dc42784cf147c0c48a680349fa168899"
G_IMAGE = "fe87c0e1cc204ed48ad3b37840f39efc"
G_TMP = "f4688fdb7df04437aeb418b961361dc5"
G_RAWIMAGE = "1344c3c82d62a2a41a3576d8abb8e3ea"
G_FONT = "c3774d7cd49de2644a7da831f34df7de"
FONT_MAT = "1028813429249325394"
G_VIEW_QR = "e9fe660734d544b4ba62c0970a43885d"      # ViewVideoQR
G_VIEW_OBRIGADO = "735d61442ef14698bdf16137a1886798"  # ViewObrigado

_counter = [1000000000]
def nid():
    _counter[0] += 1
    return _counter[0]

def go(id_, name, comps):
    s = (f"--- !u!1 &{id_}\nGameObject:\n"
         "  m_ObjectHideFlags: 0\n  m_CorrespondingSourceObject: {fileID: 0}\n"
         "  m_PrefabInstance: {fileID: 0}\n  m_PrefabAsset: {fileID: 0}\n"
         "  serializedVersion: 6\n  m_Component:\n")
    for c in comps:
        s += f"  - component: {{fileID: {c}}}\n"
    s += (f"  m_Layer: 5\n  m_Name: {name}\n  m_TagString: Untagged\n"
          "  m_Icon: {fileID: 0}\n  m_NavMeshLayer: 0\n  m_StaticEditorFlags: 0\n  m_IsActive: 1\n")
    return s

def rect(id_, gid, father, children, amin, amax, pos, size):
    ch = "".join(f"  - {{fileID: {c}}}\n" for c in children)
    ch = ("  m_Children:\n" + ch) if children else "  m_Children: []\n"
    return (f"--- !u!224 &{id_}\nRectTransform:\n"
            "  m_ObjectHideFlags: 0\n  m_CorrespondingSourceObject: {fileID: 0}\n"
            "  m_PrefabInstance: {fileID: 0}\n  m_PrefabAsset: {fileID: 0}\n"
            f"  m_GameObject: {{fileID: {gid}}}\n"
            "  m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}\n"
            "  m_LocalPosition: {x: 0, y: 0, z: 0}\n"
            "  m_LocalScale: {x: 1, y: 1, z: 1}\n"
            "  m_ConstrainProportionsScale: 0\n"
            + ch +
            f"  m_Father: {{fileID: {father}}}\n"
            "  m_LocalEulerAnglesHint: {x: 0, y: 0, z: 0}\n"
            f"  m_AnchorMin: {{x: {amin[0]}, y: {amin[1]}}}\n"
            f"  m_AnchorMax: {{x: {amax[0]}, y: {amax[1]}}}\n"
            f"  m_AnchoredPosition: {{x: {pos[0]}, y: {pos[1]}}}\n"
            f"  m_SizeDelta: {{x: {size[0]}, y: {size[1]}}}\n"
            "  m_Pivot: {x: 0.5, y: 0.5}\n")

def canvas(id_, gid):
    return (f"--- !u!223 &{id_}\nCanvas:\n"
            "  m_ObjectHideFlags: 0\n  m_CorrespondingSourceObject: {fileID: 0}\n"
            "  m_PrefabInstance: {fileID: 0}\n  m_PrefabAsset: {fileID: 0}\n"
            f"  m_GameObject: {{fileID: {gid}}}\n  m_Enabled: 1\n  serializedVersion: 3\n"
            "  m_RenderMode: 0\n  m_Camera: {fileID: 0}\n  m_PlaneDistance: 100\n"
            "  m_PixelPerfect: 0\n  m_ReceivesEvents: 1\n  m_OverrideSorting: 0\n"
            "  m_OverridePixelPerfect: 0\n  m_SortingBucketNormalizedSize: 0\n"
            "  m_VertexColorAlwaysGammaSpace: 1\n  m_AdditionalShaderChannelsFlag: 25\n"
            "  m_UpdateRectTransformForStandalone: 0\n  m_SortingLayerID: 0\n"
            "  m_SortingOrder: 0\n  m_TargetDisplay: 0\n")

def canvasgroup(id_, gid):
    return (f"--- !u!225 &{id_}\nCanvasGroup:\n"
            "  m_ObjectHideFlags: 0\n  m_CorrespondingSourceObject: {fileID: 0}\n"
            "  m_PrefabInstance: {fileID: 0}\n  m_PrefabAsset: {fileID: 0}\n"
            f"  m_GameObject: {{fileID: {gid}}}\n  m_Enabled: 1\n  m_Alpha: 1\n"
            "  m_Interactable: 1\n  m_BlocksRaycasts: 1\n  m_IgnoreParentGroups: 0\n")

def raycaster(id_, gid):
    return (f"--- !u!114 &{id_}\nMonoBehaviour:\n"
            "  m_ObjectHideFlags: 0\n  m_CorrespondingSourceObject: {fileID: 0}\n"
            "  m_PrefabInstance: {fileID: 0}\n  m_PrefabAsset: {fileID: 0}\n"
            f"  m_GameObject: {{fileID: {gid}}}\n  m_Enabled: 1\n  m_EditorHideFlags: 0\n"
            f"  m_Script: {{fileID: 11500000, guid: {G_RAYCASTER}, type: 3}}\n"
            "  m_Name:\n  m_EditorClassIdentifier:\n  m_IgnoreReversedGraphics: 1\n"
            "  m_BlockingObjects: 0\n  m_BlockingMask:\n    serializedVersion: 2\n    m_Bits: 4294967295\n")

def canvasrenderer(id_, gid):
    return (f"--- !u!222 &{id_}\nCanvasRenderer:\n"
            "  m_ObjectHideFlags: 0\n  m_CorrespondingSourceObject: {fileID: 0}\n"
            "  m_PrefabInstance: {fileID: 0}\n  m_PrefabAsset: {fileID: 0}\n"
            f"  m_GameObject: {{fileID: {gid}}}\n  m_CullTransparentMesh: 1\n")

def image(id_, gid):
    return (f"--- !u!114 &{id_}\nMonoBehaviour:\n"
            "  m_ObjectHideFlags: 0\n  m_CorrespondingSourceObject: {fileID: 0}\n"
            "  m_PrefabInstance: {fileID: 0}\n  m_PrefabAsset: {fileID: 0}\n"
            f"  m_GameObject: {{fileID: {gid}}}\n  m_Enabled: 1\n  m_EditorHideFlags: 0\n"
            f"  m_Script: {{fileID: 11500000, guid: {G_IMAGE}, type: 3}}\n"
            "  m_Name:\n  m_EditorClassIdentifier:\n  m_Material: {fileID: 0}\n"
            "  m_Color: {r: 1, g: 1, b: 1, a: 1}\n  m_RaycastTarget: 1\n"
            "  m_RaycastPadding: {x: 0, y: 0, z: 0, w: 0}\n  m_Maskable: 1\n"
            "  m_OnCullStateChanged:\n    m_PersistentCalls:\n      m_Calls: []\n"
            "  m_Sprite: {fileID: 0}\n  m_Type: 0\n  m_PreserveAspect: 0\n  m_FillCenter: 1\n"
            "  m_FillMethod: 4\n  m_FillAmount: 1\n  m_FillClockwise: 1\n  m_FillOrigin: 0\n"
            "  m_UseSpriteMesh: 0\n  m_PixelsPerUnitMultiplier: 1\n")

def rawimage(id_, gid):
    return (f"--- !u!114 &{id_}\nMonoBehaviour:\n"
            "  m_ObjectHideFlags: 0\n  m_CorrespondingSourceObject: {fileID: 0}\n"
            "  m_PrefabInstance: {fileID: 0}\n  m_PrefabAsset: {fileID: 0}\n"
            f"  m_GameObject: {{fileID: {gid}}}\n  m_Enabled: 1\n  m_EditorHideFlags: 0\n"
            f"  m_Script: {{fileID: 11500000, guid: {G_RAWIMAGE}, type: 3}}\n"
            "  m_Name:\n  m_EditorClassIdentifier:\n  m_Material: {fileID: 0}\n"
            "  m_Color: {r: 1, g: 1, b: 1, a: 1}\n  m_RaycastTarget: 1\n"
            "  m_RaycastPadding: {x: 0, y: 0, z: 0, w: 0}\n  m_Maskable: 1\n"
            "  m_OnCullStateChanged:\n    m_PersistentCalls:\n      m_Calls: []\n"
            "  m_Texture: {fileID: 0}\n  m_UVRect:\n    serializedVersion: 2\n"
            "    x: 0\n    y: 0\n    width: 1\n    height: 1\n")

def videoplayer(id_, gid):
    return (f"--- !u!328 &{id_}\nVideoPlayer:\n"
            "  m_ObjectHideFlags: 0\n  m_CorrespondingSourceObject: {fileID: 0}\n"
            "  m_PrefabInstance: {fileID: 0}\n  m_PrefabAsset: {fileID: 0}\n"
            f"  m_GameObject: {{fileID: {gid}}}\n  m_Enabled: 1\n  m_VideoClip: {{fileID: 0}}\n"
            "  m_TargetCameraAlpha: 1\n  m_TargetCamera3DLayout: 0\n  m_TargetCamera: {fileID: 0}\n"
            "  m_TargetTexture: {fileID: 0}\n  m_TimeReference: 0\n  m_TargetMaterialRenderer: {fileID: 0}\n"
            "  m_TargetMaterialProperty: <noninit>\n  m_RenderMode: 2\n  m_AspectRatio: 2\n"
            "  m_DataSource: 0\n  m_TimeUpdateMode: 2\n  m_PlaybackSpeed: 1\n  m_AudioOutputMode: 2\n"
            "  m_TargetAudioSources:\n  - {fileID: 0}\n  m_DirectAudioVolumes:\n  - 1\n  m_Url:\n"
            "  m_EnabledAudioTracks: 01\n  m_DirectAudioMutes: 00\n  m_ControlledAudioTrackCount: 1\n"
            "  m_PlayOnAwake: 1\n  m_SkipOnDrop: 1\n  m_Looping: 0\n  m_WaitForFirstFrame: 1\n"
            "  m_FrameReadyEventEnabled: 0\n  m_VideoShaders: []\n")

def tmp(id_, gid, text, fsize, halign=2, valign=512):
    return (f"--- !u!114 &{id_}\nMonoBehaviour:\n"
            "  m_ObjectHideFlags: 0\n  m_CorrespondingSourceObject: {fileID: 0}\n"
            "  m_PrefabInstance: {fileID: 0}\n  m_PrefabAsset: {fileID: 0}\n"
            f"  m_GameObject: {{fileID: {gid}}}\n  m_Enabled: 1\n  m_EditorHideFlags: 0\n"
            f"  m_Script: {{fileID: 11500000, guid: {G_TMP}, type: 3}}\n"
            "  m_Name:\n  m_EditorClassIdentifier:\n  m_Material: {fileID: 0}\n"
            "  m_Color: {r: 1, g: 1, b: 1, a: 1}\n  m_RaycastTarget: 1\n"
            "  m_RaycastPadding: {x: 0, y: 0, z: 0, w: 0}\n  m_Maskable: 1\n"
            "  m_OnCullStateChanged:\n    m_PersistentCalls:\n      m_Calls: []\n"
            f"  m_text: {text}\n  m_isRightToLeft: 0\n"
            f"  m_fontAsset: {{fileID: 11400000, guid: {G_FONT}, type: 2}}\n"
            f"  m_sharedMaterial: {{fileID: {FONT_MAT}, guid: {G_FONT}, type: 2}}\n"
            "  m_fontSharedMaterials: []\n  m_fontMaterial: {fileID: 0}\n  m_fontMaterials: []\n"
            "  m_fontColor32:\n    serializedVersion: 2\n    rgba: 4294967295\n"
            "  m_fontColor: {r: 1, g: 1, b: 1, a: 1}\n  m_enableVertexGradient: 0\n  m_colorMode: 3\n"
            "  m_fontColorGradient:\n    topLeft: {r: 1, g: 1, b: 1, a: 1}\n    topRight: {r: 1, g: 1, b: 1, a: 1}\n"
            "    bottomLeft: {r: 1, g: 1, b: 1, a: 1}\n    bottomRight: {r: 1, g: 1, b: 1, a: 1}\n"
            "  m_fontColorGradientPreset: {fileID: 0}\n  m_spriteAsset: {fileID: 0}\n"
            "  m_tintAllSprites: 0\n  m_StyleSheet: {fileID: 0}\n  m_TextStyleHashCode: -1183493901\n"
            "  m_overrideHtmlColors: 0\n  m_faceColor:\n    serializedVersion: 2\n    rgba: 4294967295\n"
            f"  m_fontSize: {fsize}\n  m_fontSizeBase: {fsize}\n  m_fontWeight: 400\n  m_enableAutoSizing: 0\n"
            "  m_fontSizeMin: 18\n  m_fontSizeMax: 72\n  m_fontStyle: 0\n"
            f"  m_HorizontalAlignment: {halign}\n  m_VerticalAlignment: {valign}\n  m_textAlignment: 65535\n"
            "  m_characterSpacing: 0\n  m_wordSpacing: 0\n  m_lineSpacing: 0\n  m_lineSpacingMax: 0\n"
            "  m_paragraphSpacing: 0\n  m_charWidthMaxAdj: 0\n  m_TextWrappingMode: 1\n"
            "  m_wordWrappingRatios: 0.4\n  m_overflowMode: 0\n  m_linkedTextComponent: {fileID: 0}\n"
            "  parentLinkedComponent: {fileID: 0}\n  m_enableKerning: 0\n  m_ActiveFontFeatures: 6e72656b\n"
            "  m_enableExtraPadding: 0\n  checkPaddingRequired: 0\n  m_isRichText: 1\n"
            "  m_EmojiFallbackSupport: 1\n  m_parseCtrlCharacters: 1\n  m_isOrthographic: 1\n"
            "  m_isCullingEnabled: 0\n  m_horizontalMapping: 0\n  m_verticalMapping: 0\n  m_uvLineOffset: 0\n"
            "  m_geometrySortingOrder: 0\n  m_IsTextObjectScaleStatic: 0\n  m_VertexBufferAutoSizeReduction: 0\n"
            "  m_useMaxVisibleDescender: 1\n  m_pageToDisplay: 1\n  m_margin: {x: 0, y: 0, z: 0, w: 0}\n"
            "  m_isUsingLegacyAnimationComponent: 0\n  m_isVolumetricText: 0\n  m_hasFontAssetChanged: 0\n"
            "  m_baseMaterial: {fileID: 0}\n  m_maskOffset: {x: 0, y: 0, z: 0, w: 0}\n")

HEADER = "%YAML 1.1\n%TAG !u! tag:unity3d.com,2011:\n"

def build_root(name, script_block_fn):
    """Cria root GO + Canvas/CanvasGroup/Raycaster e devolve (blocos, root_go, root_rt, script_id)."""
    root_go = nid(); root_rt = nid(); root_canvas = nid(); root_cg = nid(); root_ray = nid(); root_script = nid()
    blocks = []
    children = []  # preenchido depois pelo caller
    return dict(go=root_go, rt=root_rt, canvas=root_canvas, cg=root_cg, ray=root_ray, script=root_script,
                children=children, blocks=blocks, name=name)

def emit_text_child(parent_rt, text, fsize, pos, size):
    g = nid(); rt = nid(); cr = nid(); t = nid()
    blocks = [go(g, "Text", [rt, cr, t]),
              rect(rt, g, parent_rt, [], (0.5, 0.5), (0.5, 0.5), pos, size),
              canvasrenderer(cr, g), tmp(t, g, text, fsize)]
    return g, rt, t, blocks

def emit_panel(parent_rt, name):
    g = nid(); rt = nid()
    blocks = [go(g, name, [rt]), rect(rt, g, parent_rt, [], (0, 0), (1, 1), (0, 0), (0, 0))]
    return g, rt, blocks

def emit_rawimage_child(parent_rt, name, pos, size, with_video=False):
    g = nid(); rt = nid(); cr = nid(); ri = nid()
    comps = [rt, cr, ri]
    vp = None
    if with_video:
        vp = nid(); comps.append(vp)
    blocks = [go(g, name, comps), rect(rt, g, parent_rt, [], (0.5, 0.5), (0.5, 0.5), pos, size),
              canvasrenderer(cr, g), rawimage(ri, g)]
    if with_video:
        blocks.append(videoplayer(vp, g))
    return g, rt, ri, vp, blocks

def gen_obrigado():
    r = build_root("View - Obrigado", None)
    # Background
    bg_go = nid(); bg_rt = nid(); bg_cr = nid(); bg_img = nid()
    # Title
    title_go, title_rt, title_tmp, title_blocks = emit_text_child(r['rt'], '"Obrigado!"', 120, (0, 0), (900, 200))
    r['children'] = [bg_rt, title_rt]
    out = HEADER
    out += go(r['go'], r['name'], [r['rt'], r['canvas'], r['cg'], r['ray'], r['script']])
    out += rect(r['rt'], r['go'], 0, r['children'], (0, 0), (1, 1), (0, 0), (0, 0))
    out += canvas(r['canvas'], r['go']) + canvasgroup(r['cg'], r['go']) + raycaster(r['ray'], r['go'])
    # script ViewObrigado
    out += (f"--- !u!114 &{r['script']}\nMonoBehaviour:\n"
            "  m_ObjectHideFlags: 0\n  m_CorrespondingSourceObject: {fileID: 0}\n"
            "  m_PrefabInstance: {fileID: 0}\n  m_PrefabAsset: {fileID: 0}\n"
            f"  m_GameObject: {{fileID: {r['go']}}}\n  m_Enabled: 1\n  m_EditorHideFlags: 0\n"
            f"  m_Script: {{fileID: 11500000, guid: {G_VIEW_OBRIGADO}, type: 3}}\n"
            "  m_Name:\n  m_EditorClassIdentifier:\n"
            f"  m_BackgroundImage: {{fileID: {bg_img}}}\n"
            f"  m_TitleText: {{fileID: {title_tmp}}}\n"
            "  m_AutoReturnSeconds: 8\n")
    # Background blocks
    out += go(bg_go, "Background", [bg_rt, bg_cr, bg_img])
    out += rect(bg_rt, bg_go, r['rt'], [], (0, 0), (1, 1), (0, 0), (0, 0))
    out += canvasrenderer(bg_cr, bg_go) + image(bg_img, bg_go)
    out += "".join(title_blocks)
    return out

def gen_qr():
    r = build_root("View - QR", None)
    # Background
    bg_go = nid(); bg_rt = nid(); bg_cr = nid(); bg_img = nid()
    # Loading panel + text
    load_go, load_rt, load_blocks = emit_panel(r['rt'], "Loading")
    lt_go, lt_rt, lt_tmp, lt_blocks = emit_text_child(load_rt, '"Enviando seu v\\xEDdeo..."', 56, (0, 0), (900, 120))
    # Content panel
    content_go, content_rt, content_blocks = emit_panel(r['rt'], "Content")
    # Video preview (RawImage + VideoPlayer)
    vid_go, vid_rt, vid_ri, vid_vp, vid_blocks = emit_rawimage_child(content_rt, "VideoPreview", (0, 120), (600, 1066), with_video=True)
    # QR panel + QR rawimage
    qrp_go, qrp_rt, qrp_blocks = emit_panel(content_rt, "QR Panel")
    qr_go, qr_rt, qr_ri, _, qr_blocks = emit_rawimage_child(qrp_rt, "QRCode", (0, -650), (420, 420), with_video=False)

    r['children'] = [bg_rt, load_rt, content_rt]
    out = HEADER
    out += go(r['go'], r['name'], [r['rt'], r['canvas'], r['cg'], r['ray'], r['script']])
    out += rect(r['rt'], r['go'], 0, r['children'], (0, 0), (1, 1), (0, 0), (0, 0))
    out += canvas(r['canvas'], r['go']) + canvasgroup(r['cg'], r['go']) + raycaster(r['ray'], r['go'])
    # script ViewVideoQR
    out += (f"--- !u!114 &{r['script']}\nMonoBehaviour:\n"
            "  m_ObjectHideFlags: 0\n  m_CorrespondingSourceObject: {fileID: 0}\n"
            "  m_PrefabInstance: {fileID: 0}\n  m_PrefabAsset: {fileID: 0}\n"
            f"  m_GameObject: {{fileID: {r['go']}}}\n  m_Enabled: 1\n  m_EditorHideFlags: 0\n"
            f"  m_Script: {{fileID: 11500000, guid: {G_VIEW_QR}, type: 3}}\n"
            "  m_Name:\n  m_EditorClassIdentifier:\n"
            f"  m_VideoPlayer: {{fileID: {vid_vp}}}\n"
            f"  m_VideoDisplay: {{fileID: {vid_ri}}}\n"
            "  m_VideoRenderTexture: {fileID: 0}\n"
            f"  m_QrCodePanel: {{fileID: {qrp_go}}}\n"
            f"  m_QrCodeDisplay: {{fileID: {qr_ri}}}\n"
            f"  m_LoadingPanel: {{fileID: {load_go}}}\n"
            f"  m_ContentPanel: {{fileID: {content_go}}}\n"
            "  m_ServerUrl: https://api.tmj.dilis.com.br\n"
            "  m_UploadEndpoint: /agent/participants/upload\n"
            "  m_RedirectLink:\n")
    out += go(bg_go, "Background", [bg_rt, bg_cr, bg_img])
    out += rect(bg_rt, bg_go, r['rt'], [], (0, 0), (1, 1), (0, 0), (0, 0))
    out += canvasrenderer(bg_cr, bg_go) + image(bg_img, bg_go)
    # loading
    out += go(load_go, "Loading", [load_rt])
    out += rect(load_rt, load_go, r['rt'], [lt_rt], (0, 0), (1, 1), (0, 0), (0, 0))
    out += "".join(lt_blocks)
    # content
    out += go(content_go, "Content", [content_rt])
    out += rect(content_rt, content_go, r['rt'], [vid_rt, qrp_rt], (0, 0), (1, 1), (0, 0), (0, 0))
    out += "".join(vid_blocks)
    # qr panel
    out += go(qrp_go, "QR Panel", [qrp_rt])
    out += rect(qrp_rt, qrp_go, content_rt, [qr_rt], (0, 0), (1, 1), (0, 0), (0, 0))
    out += "".join(qr_blocks)
    return out

if __name__ == "__main__":
    which, path = sys.argv[1], sys.argv[2]
    text = gen_obrigado() if which == "obrigado" else gen_qr()
    with open(path, "w", encoding="utf-8", newline="\n") as f:
        f.write(text)
    print("OK:", path)
