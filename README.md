# Unity Asset Validation Tool

一个用于 Unity 美术资源检查、导入设置验证、安全自动修复和报告导出的 Editor 工具。

本项目面向技术美术 / TA 工具与管线方向，目标是在美术资源进入项目时，提前发现命名、贴图、模型、材质和 Prefab 中的常见问题，减少人工检查成本，提高资源接入流程的稳定性。

---

## 项目预览

> 可在此处放置工具截图或演示 GIF。

![Tool Window](Screenshots/tool_window.png)

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

| 资源类型 | 检查内容 | 是否支持自动修复 |
|---|---|---|
| Texture | 贴图命名前缀，例如 `T_` | 是 |
| Texture | Max Size 超过配置上限 | 是 |
| Texture | Normal 贴图未设置为 Normal Map | 是 |
| Model | 模型命名前缀，例如 `SM_` / `CH_` | 是 |
| Model | Scale Factor 不为 1 | 是 |
| Model | Read/Write Enabled 被开启 | 是 |
| Model | 材质槽为空 | 否 |
| Material | Shader 缺失或错误 | 否 |
| Prefab | 根节点名称与文件名不一致 | 否 |
| Prefab | 材质槽为空 | 否 |
| Prefab | Missing Script | 否 |
| Prefab | Missing Reference | 否 |

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
