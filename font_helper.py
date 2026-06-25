#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
XUnity.AutoTranslator 字体检测工具
检测系统中支持中文（CJK）的字体，方便配置 OverrideFont 解决方块字问题。

依赖：
  - pip install fonttools matplotlib

用法：
  python font_helper.py
"""

import sys
import os
import subprocess


def try_install_module(module_name, package_name=None):
    """尝试导入模块，失败时自动安装。"""
    if package_name is None:
        package_name = module_name
    try:
        __import__(module_name)
        return True
    except ImportError:
        print(f"[信息] 未检测到 {package_name}，正在尝试自动安装...")
        try:
            subprocess.check_call([sys.executable, "-m", "pip", "install", package_name],
                                  stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
            print(f"[成功] {package_name} 安装完成。")
            return True
        except Exception:
            print(f"[失败] 无法自动安装 {package_name}，请手动运行：pip install {package_name}")
            return False


def get_fonts_via_matplotlib():
    """通过 matplotlib 获取系统中所有字体信息。"""
    from matplotlib import font_manager
    fonts = []
    for font in font_manager.fontManager.ttflist:
        # 去重：同名字体只保留一个
        if not any(f['name'] == font.name for f in fonts):
            fonts.append({
                'name': font.name,
                'file': getattr(font, 'fname', '')
            })
    return fonts


def check_cjk_support(font_path):
    """
    使用 fontTools 检查字体文件是否包含 CJK 字符。
    检查范围：CJK Unified Ideographs (U+4E00 - U+9FFF)
    """
    try:
        from fontTools.ttLib import TTFont

        font = TTFont(font_path)
        has_cjk = False

        # 遍历所有 cmap 表
        for table in font.get('cmap', {}).tables:
            if not table.cmap:
                continue
            # 检查是否包含任何 CJK 统一表意文字
            for code in table.cmap.keys():
                if 0x4E00 <= code <= 0x9FFF:
                    has_cjk = True
                    break
                # 额外检查 CJK 扩展 A 区（U+3400 - U+4DBF）
                if 0x3400 <= code <= 0x4DBF:
                    has_cjk = True
                    break
            if has_cjk:
                break

        font.close()
        return has_cjk
    except Exception as e:
        # 无法解析的字体文件，忽略
        return False


def heuristic_cjk_font_name(font_name):
    """
    基于字体名称的启发式判断（当 fontTools 不可用时作为备选）。
    """
    cjk_keywords = [
        'chinese', 'cjk', 'han', 'hei', 'song', 'ming', 'kai',
        'yahei', 'microsoft', 'simsun', 'simhei', 'simsun',
        'noto', 'source', 'adobe', 'wqy', 'wenquanyi',
        'jhenghei', 'meiryo', 'yu gothic', 'gulim', 'dotum',
        'malgun', 'batang', 'mikiyang', 'pcmyungjo',
        'ms mincho', 'ms gothic', 'hgrs', 'hggt',
        'stxihei', 'stheiti', 'stsong', 'stkaiti', 'stfangson',
        'lihei', 'lisong', 'pingfang', 'heiti', 'songti',
        'dengxian', 'fangsong', 'kaiti', 'youyuan', 'lishu',
        '方正', '思源', '黑体', '宋体', '楷体', '仿宋', '微软雅黑',
        '文泉驿', '苹方', '华文', '幼圆', '隶书', '新宋体',
        '等线', '圆体', '明体', 'ゴシック', '明朝', 'gothic', 'mincho'
    ]
    lower_name = font_name.lower()
    return any(kw in lower_name for kw in cjk_keywords)


def main():
    print("=" * 60)
    print(" XUnity.AutoTranslator 中文字体检测工具")
    print("=" * 60)
    print()

    # 检查依赖
    has_matplotlib = try_install_module('matplotlib', 'matplotlib')
    has_fonttools = try_install_module('fontTools', 'fonttools')

    if not has_matplotlib:
        print("\n[错误] matplotlib 是必需的，请手动安装：pip install matplotlib")
        return 1

    print("[信息] 正在扫描系统字体...\n")

    fonts = get_fonts_via_matplotlib()
    cjk_fonts = []
    heuristic_fonts = []

    if has_fonttools:
        print(f"[信息] 共扫描到 {len(fonts)} 个字体，使用 fontTools 精确检测 CJK 支持...\n")
    else:
        print(f"[信息] 共扫描到 {len(fonts)} 个字体，fontTools 未安装，使用名称启发式检测...\n")

    for font in fonts:
        font_name = font['name']
        font_file = font['file']

        if not font_file or not os.path.exists(font_file):
            continue

        if has_fonttools:
            if check_cjk_support(font_file):
                cjk_fonts.append(font_name)
        else:
            if heuristic_cjk_font_name(font_name):
                heuristic_fonts.append(font_name)

    if has_fonttools:
        if cjk_fonts:
            print("✅ 以下字体支持中文（CJK），可填入 Config.ini 的 OverrideFont：")
            print("-" * 60)
            for name in sorted(set(cjk_fonts)):
                print(f"  {name}")
            print("-" * 60)
            print(f"\n共找到 {len(set(cjk_fonts))} 个支持中文的字体。")
        else:
            print("⚠️ 未找到支持中文的字体，请检查是否安装了中文字体。")
    else:
        if heuristic_fonts:
            print("⚠️ 以下字体可能是中文字体（基于名称启发式判断，可能不准确）：")
            print("-" * 60)
            for name in sorted(set(heuristic_fonts)):
                print(f"  {name}")
            print("-" * 60)
            print(f"\n共找到 {len(set(heuristic_fonts))} 个疑似中文字体。")
            print("\n[建议] 安装 fonttools 可获得更精确的结果：pip install fonttools")
        else:
            print("⚠️ 未找到疑似中文字体。")

    print()
    print("=" * 60)
    print(" 配置示例（Config.ini）")
    print("=" * 60)
    print("""
[Behaviour]
; 将以下字体名称填入 OverrideFont
OverrideFont=Microsoft YaHei

; 可选：覆盖字体大小
OverrideFontSize=18
""")

    print("=" * 60)
    print(" 提示")
    print("=" * 60)
    print("""
1. 字体名称必须完全匹配系统字体名称（区分大小写）。
2. 游戏中按 ALT + F 可切换 OverrideFont 的开启/关闭。
3. 如果翻译后的中文仍然显示方块，说明字体配置不正确或游戏
   使用自定义字体渲染系统，需尝试其他字体名称。
4. 最稳妥的推荐：Windows 用户直接使用 Microsoft YaHei。
""")

    return 0


if __name__ == '__main__':
    sys.exit(main())
