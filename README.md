# ExTool HDA助手（Unity版本）
## 介绍
本插件自带集成了Sidfx官方的Houdini Engine For Unity（我挺奇怪的，Sidfx明明git仓库是公开的，但就是没做成package。搞得我不得不完整拷贝了一份进插件。）
同时增加了以下可视化的操作：

**1.加载HDA（包括工程内和工程外的hda，hdalc资源）**

**2.清除Houdini Engine Asset Cache**

**3.指定目录的hda烘焙**

**4.指定Gamobject的替换烘焙（场景内Gameobject 和 预制体prefab都可替换）**

**5.重置HDA编辑环境，方便快速编辑使用下一个HDA**

**6.HDA Inspector窗口分离到HDA助手窗口上，不用再费劲的每次锁定HDA物件的Inspector窗口了。**

另外还优化了工作流：

**1.简化了需要手动建立会话的操作，只要一打开HDA助手就自行建立连接。**

**2.打开HDA助手时会自动切换到空场景，关闭HDA助手时会自动切换到打开前的场景。从而不影响原先的工作场景，分离出一个给编辑HDA用的场景。**

## 快速开始
### 加载方式：

*1.（建议使用）复制本仓库url，使用Package Manager的Add package from gitURL加载*

*2.（不建议）克隆本仓库到本地项目*

### 使用方法：
**点击EXTool->Houdini->HDA Assistant Tool**

我觉得界面还是一幕了然的，提示应该都很清楚。

## 后续
其实这个插件内容很少，花了一天顺手做的。主要还是Sidfx的Houdini Engine For Unity做得有点偷懒，用起来很麻烦，几个有用的接口就是不给可视化的界面。
那我还不如自己集成一下。

后续可能会持续更新一些别的功能。

*“1.简化了需要手动建立会话的操作，只要一打开HDA助手就自行建立连接。”* 这条可能是有bug的，我自己并没有完整的测试完这块功能。






*游戏PCG交流（水）群：813530330*
