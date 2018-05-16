# PirateX.Deploy

PirateX.Deploy 是windows平台下轻量级部署工具。能够通过简单的命令脚本来实现批量远程部署程序。包括站点、WinService以及其他部署相关的命令。

## 程序介绍

1、PirateX.Deploy.Agent

部署在目标机器上的代理程序，用以接收指令，执行相应的任务

2、PirateX.Deploy.Client

指令发起程序，用以向远端Agent发起指令

## 安装和运行

### 安装 PirateX.Deploy.Agent

>PirateX.Deploy.Agent install
>
>net start PirateX.Deploy.Agnet

 默认情况下 PirateX.Deploy.Agent 监听  40001端口，当前目录为工作目录
 安装完成后程序会生成一个密钥文件，密钥文件用以Client和Agent通信中身份确认

 ```shell
 secretkey.txt
 ```

 >TODO 其他端口的设置待定

### 安装 PirateX.Deploy.Client

解压程序包

将目录添加到环境变量中(是个程序员都会！)

## Hello World

### test.hn

新建test.hn脚本文件，内容如下

```hn
#test message
log:"Hello World!"

//查看帮助
help:""
```

### test.bat

同脚本目录下新建批处理脚本test.bat

>例如:
>
>Agent地址: 127.0.0.1
>
>Agent密钥: 2ef9ee233b064764bdff572fbb9c60ce

```shell
PirateX.Deploy.Client 127.0.0.1 2ef9ee233b064764bdff572fbb9c60ce test.hn

pause
```

Client输出结果

```shell
2018-05-16 09:55:36.9770 PirateX.Deploy.Client Started! Press Ctrl + C to exit
2018-05-16 09:55:37.0400 PirateX.Deploy.Agent Version 1.0.0.0
2018-05-16 09:55:37.1140 >>>>>>>>>>>>>>>>processing [log] >>>>>>>>>>>>>>>>
2018-05-16 09:55:37.1140 Hello World!
2018-05-16 09:55:37.1880 log msg!
2018-05-16 09:55:37.1880 ------------------------------------
2018-05-16 09:55:37.1880 >>>>>>>>>>>>>>>>processing [help] >>>>>>>>>>>>>>>>
2018-05-16 09:55:37.1880 change-pool                    修改IIS应用程序池
2018-05-16 09:55:37.2020 change-site                    修改IIS web站点
2018-05-16 09:55:37.2179 delete-file                    删除文件
2018-05-16 09:55:37.2309 delete-pool                    删除应用程序池
2018-05-16 09:55:37.2399 delete-site                    删除站点
2018-05-16 09:55:37.2399 file-download                  下载文件
2018-05-16 09:55:37.2629 extract                        解压压缩包
2018-05-16 09:55:37.2629 install-service                安装服务
2018-05-16 09:55:37.2769 jenkins-build                  Jenkins编译项目
2018-05-16 09:55:37.2769 log                            打印信息
2018-05-16 09:55:37.2889 new-pool                       新建应用程序池
2018-05-16 09:55:37.2889 new-site                       新建站点
2018-05-16 09:55:37.2889 package-get                    获取包
2018-05-16 09:55:37.3059 replace-config                 替换配置内容
2018-05-16 09:55:37.3259 run-app                        运行程序
2018-05-16 09:55:37.3339 execute-batch                  执行批处理脚本
2018-05-16 09:55:37.3339 start-pool                     启动应用程序池
2018-05-16 09:55:37.3339 start-service                  启动服务
2018-05-16 09:55:37.3519 start-site                     启动站点
2018-05-16 09:55:37.3629 stop-pool                      停止应用程序池
2018-05-16 09:55:37.3629 stop-service                   停止服务
2018-05-16 09:55:37.3899 stop-site                      停止站点
2018-05-16 09:55:37.3969 uninstall-service              卸载服务
2018-05-16 09:55:37.3969 update-self                    更新自己
2018-05-16 09:55:37.4169 copy-file                      拷贝文件
2018-05-16 09:55:37.4269 help                           查看帮助
2018-05-16 09:55:37.4269 package-delete                 删除包信息
2018-05-16 09:55:37.4449 site-newapplication            站点新增应用程序
2018-05-16 09:55:37.4449
2018-05-16 09:55:37.4449 ------------------------------------
```


