# Unity Asset Validation Tool

一个用于 Unity 美术资源检查、导入设置验证、安全自动修复和报告导出的 Editor 工具。

本项目面向技术美术 / TA 工具与管线方向，目标是在美术资源进入项目时，提前发现命名、贴图、模型、材质和 Prefab 中的常见问题，减少人工检查成本，提高资源接入流程的稳定性。

---

## 项目预览

> 可在此处放置工具截图或演示 GIF。

<img width="754" height="409" alt="Snipaste_2026-05-23_04-53-52" src="https://github.com/user-attachments/assets/e7d8ba90-0c7b-4439-895b-a3c459c408e7" /><img width="287" height="692" alt="Snipaste_2026-05-23_04-56-12" src="https://github.com/user-attachments/assets/43d5cabc-3348-476e-9d00-963b6e3c5360" /><img width="1280" height="764" alt="Snipaste_2026-05-23_05-11-41" src="https://github.com/user-attachments/assets/a30612dc-15fb-4176-a46c-5b0bb4d6446e" /><img width="1280" height="764" alt="Snipaste_2026-05-23_05-12-20" src="https://github.com/user-attachments/assets/ddc36911-a6d3-41cf-aa5b-7d5537ecb8a9" /><img width="1280" height="764" alt="Snipaste_2026-05-23_05-12-53" src="https://github.com/user-attachments/assets/43c34f5b-aad2-4be8-bb3d-0a9f062b4629" /><img width="950" height="198" alt="Snipaste_2026-05-23_05-14-20" src="https://github.com/user-attachments/assets/7b01d0a1-1bf4-4382-bdf3-7d246a4e9bd0" /><img width="816" height="456" alt="Snipaste_2026-05-23_05-15-20" src="https://github.com/user-attachments/assets/ea1c02e2-71ef-4c0b-9eff-7a49f9b20b09" />

---
 
## 功能特性

- 支持扫描选中资源或整个 Unity 项目
- 支持检查贴图、模型、材质和 Prefab
- 支持检查贴图命名前缀、贴图 Max Size、Normal Map 类型
- 支持检查模型命名前缀、Scale Factor、Read/Write 设置
- 支持检查材质 Shader 缺失或错误
- 支持检查 Prefab 空材质槽、Missing Script、Missing Reference
- 支持部分低风险问题一键修复
- 支持导出 CSV / JSON 检查报告
- 使用 ScriptableObject 管理检查规则，便于扩展和配置

---

## 支持的检查项

| 资源类型 | 检查内容                         | 是否支持自动修复 |
| -------- | -------------------------------- | ---------------- |
| Texture  | 贴图命名前缀，例如 `T_`          | 是               |
| Texture  | Max Size 超过配置上限            | 是               |
| Texture  | Normal 贴图未设置为 Normal Map   | 是               |
| Model    | 模型命名前缀，例如 `SM_` / `CH_` | 是               |
| Model    | Scale Factor 不为 1              | 是               |
| Model    | Read/Write Enabled 被开启        | 是               |
| Model    | 材质槽为空                       | 否               |
| Material | Shader 缺失或错误                | 否               |
| Prefab   | 根节点名称与文件名不一致         | 否               |
| Prefab   | 材质槽为空                       | 否               |
| Prefab   | Missing Script                   | 否               |
| Prefab   | Missing Reference                | 否               |

---

## 项目目的

在实际项目中，美术资源进入 Unity 后经常会出现以下问题：

- 资源命名不符合规范
- 贴图尺寸过大
- Normal 贴图类型设置错误
- 模型 Scale Factor 不统一
- 模型 Read/Write 误开，造成额外内存开销
- 材质槽为空
- Prefab 存在 Missing Script 或 Missing Reference
- 材质 Shader 丢失导致粉色材质

这些问题如果靠人工检查，效率低，也容易漏掉。本工具将资源检查前置到导入阶段，通过自动扫描、问题分类、低风险自动修复和报告导出，帮助团队更早发现资源问题。

---

## 安装方式

将以下目录复制到 Unity 工程中：

```text
Assets/Editor/AssetValidationTool/
```

该工具是 Unity Editor 工具，必须放在 `Editor` 文件夹下。

推荐 Unity 版本：

```
Unity 2022.x 或更高版本
```

------

## 使用方式

在 Unity 菜单栏打开工具：

```
TA Tools > Asset Validation > Open Tool
```

基本流程：

1. 打开 Asset Validation 工具窗口。
2. 选择规则配置文件，或让工具自动创建默认配置。
3. 选择扫描范围：选中资源或整个项目。
4. 点击 `Scan` 开始扫描。
5. 查看 Error / Warning / Info 检查结果。
6. 对支持修复的问题点击 `Fix`，或使用 `Auto Fix All Fixable` 批量修复。
7. 根据需要导出 CSV / JSON 报告。

------

## 规则配置

检查规则通过 `ScriptableObject` 管理。

默认配置路径：

```
Assets/Editor/AssetValidationTool/Config/DefaultAssetValidationRuleConfig.asset
```

可配置内容包括：

- 贴图命名前缀
- 模型命名前缀
- 模型自动修复默认前缀
- 最大贴图尺寸
- Normal 贴图命名关键字
- 模型 Scale Factor 规则
- 模型 Read/Write 规则
- Prefab 缺失引用检查
- 允许的资源目录范围

------

## 项目架构

项目采用模块化检查器与修复器架构。

```
EditorWindow
    ↓
AssetValidationRunner
    ↓
IAssetChecker
    ↓
AssetValidationResult
    ↓
AssetValidationAutoFixer
    ↓
具体 Fixer
```

主要目录结构：

```
Assets/Editor/AssetValidationTool/
├─ Core/
├─ Config/
├─ Checkers/
├─ Fixers/
├─ Report/
└─ UI/
```

各模块职责：

| 目录     | 职责                                           |
| -------- | ---------------------------------------------- |
| Core     | 基础数据结构、检查结果、扫描调度、自动修复分发 |
| Config   | 检查规则配置和默认配置创建                     |
| Checkers | 贴图、模型、材质、Prefab 的具体检查逻辑        |
| Fixers   | 命名、贴图导入设置、模型导入设置的安全修复逻辑 |
| Report   | CSV / JSON 报告导出                            |
| UI       | Unity EditorWindow 工具界面                    |

------

## 设计思路

本工具将“检查问题”和“修改资源”分离。

`Checker` 只负责发现问题，并生成统一的 `AssetValidationResult`。
 `Fixer` 只负责处理明确、低风险、可确定的自动修复。
 `EditorWindow` 只负责显示结果、筛选结果和触发操作。

支持自动修复的问题包括：

- 自动补充贴图命名前缀
- 自动补充模型命名前缀
- 限制贴图 Max Size
- 将 Normal 贴图设置为 Normal Map
- 将模型 Scale Factor 设置为 1
- 关闭模型 Read/Write Enabled

不自动修复的问题包括：

- 空材质槽
- Missing Script
- Missing Reference
- Shader 缺失
- 路径归类问题

这些问题通常需要人工判断，工具只负责定位和报告，避免误改资源。

------

## 技术点

- 使用 `EditorWindow` 搭建 Unity 编辑器工具界面
- 使用 `AssetDatabase` 实现资源扫描、加载和重命名
- 使用 `TextureImporter` 检查并修复贴图导入设置
- 使用 `ModelImporter` 检查并修复模型导入设置
- 使用 `PrefabUtility.LoadPrefabContents` 安全检查 Prefab 内容
- 使用 `SerializedObject` 和 `SerializedProperty` 检测 Missing Reference
- 使用 `ScriptableObject` 管理可配置检查规则
- 使用 `JsonUtility` 和 CSV 写入实现报告导出
- 使用接口式 `IAssetChecker` 架构提升扩展性

------

## 示例问题

工具可以检测类似问题：

```
Wood_BaseColor.png 未以 T_ 开头
T_Wood_Normal.png 未设置为 Normal Map
Barrel.fbx 未以 SM_ / CH_ 开头
SM_Barrel.fbx 的 Scale Factor 不为 1
SM_Chair.fbx 开启了 Read/Write Enabled
P_Barrel.prefab 存在 Missing Script
P_House.prefab 存在 Missing Reference
```

自动修复后，部分问题会被直接处理，例如：

```
Wood_BaseColor.png → T_Wood_BaseColor.png
Barrel.fbx → SM_Barrel.fbx
Texture Max Size: 4096 → 2048
Texture Type: Default → Normal Map
Model Scale Factor: 0.01 → 1
Read/Write Enabled: true → false
```

------

## 后续扩展方向

- Shader 白名单检查
- Audio 导入设置检查
- Animation Clip 检查
- 移动端 / PC / 角色 / 场景资源规则预设
- 检查结果统计面板
- Markdown 报告导出
- 接入 CI，在构建前自动进行资源检查

------

## 作品集说明

该项目展示了 Unity Editor 工具开发、资源管线自动化、检查器架构设计和低风险自动修复能力。

项目重点不是单个检查规则，而是模拟真实生产流程中的资源入库检查：
 将重复、容易漏掉的人工检查流程工具化，把资源问题前置暴露，并通过可配置规则和报告导出提高团队协作效率。
