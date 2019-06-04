# H2五子棋AI引擎

人工智能课程实验课实验成果

使用C#编写，可以基于.NET Framework 4.7.2或.NET Core 2.2运行

本引擎使用Alpha-Beta剪枝搜索算法，在中局搜索深度大概为9层。

使用了杀手启发

在设计上，我力求使启发函数简单，以体现算法的力量。

关于算法的详细介绍，参考我的[课程报告论文](./paper/five-in-a-row.pdf)

## 如何运行

[下载](https://github.com/huww98/pbrain-H2/releases/latest)预编译的版本或自行编译pbrain-H2项目

[下载](http://petr.lastovicka.sweb.cz/piskvork.zip)五子棋前端，在玩家设置中选择编译好的程序。

修改config.json文件以实验不同的配置
* 将`Engine`设置为`MonteCarlo`以测试作为棋力对照的蒙特卡洛树搜索算法引擎
