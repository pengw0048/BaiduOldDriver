# API 概述

NetDisk 是实现百度网盘 API 的 C# 工程。使用者可以直接引用编译好的 NetDisk.dll。

目前版本的目标框架是 .NET Framework 4.5，引用它的工程最好也选用 .NET 4.5。

库中几乎所有函数的返回值以 Result 为基类。约定 bool success 表示是否成功完成调用流程（但不意味着结果为成功）。如果 success = false，Exception exception 保存了相关的异常。

*很多情况下，返回对象中给出的的 errno 记录了百度返回的操作结果，errno = 0 表示操作实际成功。*

网络传输使用 WebClient，它绝大多数情况下能自动选择合适的网络和（如果设置了的）代理服务器。

解决方案 BaiduOldDrive 中有若干使用 NetDisk 库的示例程序，可供参考。

# 身份验证

调用 Operations 类当中的操作之前，先要用这个类当中的函数获得登录信息。请将最终得到的 Credential 对象传入此后的操作函数。

它通过模拟网页版百度网盘的登录，获得 baiduid、bduss、stoken 这三个必需的 Cookies。

下面是命令行登录流程的例子：

```cs
var checkResult = Authentication.LoginCheck("用户名");
CheckSuccess(checkResult);
if (checkResult.needVCode)
{
 File.WriteAllBytes("vcode.png", checkResult.image);
 Console.WriteLine("输入 vcode.png 中的验证码：");
 checkResult.verifyCode = Console.ReadLine();
}
var loginResult = Authentication.Login("用户名", "密码", checkResult);
CheckSuccess(loginResult);
var credential = loginResult.credential;
// credential 变量以后操作使用
```

对于有 GUI 的程序，建议在用户名框更改时，就检查是否需要验证码，并对应显示。

## 登录检查

```cs
Authentication.LoginCheck(string username) : LoginCheckResult
```

在实际登录之前，需要先用这个函数，检查一下是否需要输入验证码。

### 参数

string username: 用户名

### 返回值

bool needVCode: 是否需要输入验证码

byte[] image: 如果需要验证码，这个数组存储了验证码图片

## 登录

```cs
Authentication.Login(string username, string password, LoginCheckResult checkResult) : LoginResult
```

对于同一个用户名，在调用 LoginCheck后，实际进行登录。如果 checkResult.needVCode == true，需将验证码文字存入 checkResult.verifyCode。

### 参数

string username: 用户名

string password: 密码

LoginCheckResult checkResult: 该用户名对应的 LoginCheck 返回结果

### 返回值

Credential credential: 表示登录用户的身份信息

int errno: 错误代码，详见 [这里](https://github.com/tiancaihb/BaiduOldDriver/blob/master/BaiduOldDriver/login_errno.txt)

## 检查身份信息是否有效

```cs
Authentication.IsLoggedIn(Credential credential) : bool
```

### 参数

Credential credential: 要检查的身份信息

### 返回值

是或否

# 获取基本信息

## 获取配额

```cs
Operation.GetQuota(Credential credential) : QuotaResult
```

### 参数

Credential credential: 身份信息

### 返回值

int errno: 错误代码

long total: 总字节数

long free: 空余字节数

long used: 使用字节数

## 获取用户信息

```cs
Operation.GetUserInfo(Credential credential) : UserInfoResult
```

### 参数

Credential credential: 身份信息

### 返回值

int errno: 错误代码

string records[0].avatar_url: 头像图片的地址

string records[0].uname: 用户昵称

string records[0].priority_name: 带星号的用户昵称

# 文件操作

## 获取文件列表

```cs
Operation.GetFileList(string path, Credential credential) : FileListResult
```

对于文件夹 path，列出其下所有的文件和文件夹。根目录是“/”，其他路径后不加斜线，例如“/Folder”。

### 参数

string path: 网盘上的文件夹全路径

Credential credential: 身份信息

### 返回值

int errno: 错误代码

FileListResult.Entry[] list: 文件（夹）信息的数组

int list.isdir: 是否为文件夹（1 或 0）

string list.path: 该项目的全路径

string list.server_filename: 该项目的名称

long list.size: 字节数

## 获得缩略图

```cs
Operation.GetThumbnail(string path, Credential credential, int width = 125, int height = 90, int quality = 100) : ThumbnailResult
```

### 参数

string path: 网盘上的文件全路径

Credential credential: 身份信息

int width: 宽度

int height: 高度

int quality: 质量

### 返回值

byte[] image: 缩略图文件

## 复制 / 移动

```cs
Operation.Copy(string path, string dest, string newname, Credential credential) : FileOperationResult
Operation.Move(string path, string dest, string newname, Credential credential) : FileOperationResult
```

如果指定的名字已经存在，会返回错误。

### 参数

string path: 网盘上的文件（夹）全路径

string dest: 目标所在文件夹的全路径

string newname: 新的名字

Credential credential: 身份信息

### 返回值

int errno: 错误代码

string path: 原文件（夹）的全路径

## 重命名

```cs
Operation.Rename(string path, string newname, Credential credential) : FileOperationResult
```

如果指定的名字已经存在，会返回错误。

### 参数

string path: 网盘上的文件（夹）全路径

string newname: 新的名字

Credential credential: 身份信息

### 返回值

int errno: 错误代码

string path: 原文件（夹）的全路径

## 删除

```cs
Operation.Delete(string path, Credential credential) : FileOperationResult
```

### 参数

string path: 网盘上的文件（夹）全路径

Credential credential: 身份信息

### 返回值

int errno: 错误代码

string path: 原文件（夹）的全路径

## 新建文件夹

```cs
Operation.CreateFolder(string path, Credential credential) : FileOperationResult
```

如果指定的名字已经存在，会自动改名，例如"/Folder(1)"。可以同时新建若干级文件夹。

### 参数

string path: 网盘上的文件夹全路径

Credential credential: 身份信息

### 返回值

int errno: 错误代码

string path: 实际创建的文件夹全路径

# 上传下载

## 获取下载地址

```cs
Operation.GetDownload(string path, Credential credential) : GetDownloadResult
```

对于一个文件通常会返回一组地址，从域名可以看出是不同地区的服务器。下载速度有时差别会很大，特别是从国外下载的时候；此外返回的 rank 不一定符合实际情况。这些地址的有效期是 8 小时。

百度网盘客户端对于非会员有某种限速，但给会员的下载地址可能也会比非会员的更快。具体情况还需进一步实验。

### 参数

string path: 网盘上的文件夹全路径

Credential credential: 身份信息

### 返回值

GetDownloadResult.Entry[] urls: 下载地址的数组

int urls.rank: 百度给的服务器优先度排名

string urls.url: 下载地址

## 简单上传

```cs
Operation.SimpleUpload(string localpath, string remotepath, Credential credential, string host = "c.pcs.baidu.com") : CommitUploadResult
```

适合小文件上传。因为实现当中会先把文件全部读入内存，并且一次 WebClient 操作的超时有限，对于较大（例如 >= 10MB）的文件，请考虑分块上传。

目前这个函数仍然是通过分块上传实现的。

### 参数

string localpath: 本地文件全路径

string remotepath: 网盘上的文件保存路径

Credential credential: 身份信息

string host: 上传服务器的地址，可以用 GetUploadServers 函数获取

### 返回值

int errno: 错误代码

其他成员类似 FileListResult.Entry

## 分块上传（简化版）

```cs
Operation.ChunkedUpload(string localpath, string remotepath, Credential credential) : CommitUploadResult
```

在内部使用 4MB 大小的分块上传指定文件。该函数会自动获取并选择第一个上传服务器。

### 参数

string localpath: 本地文件全路径

string remotepath: 网盘上的文件保存路径

Credential credential: 身份信息

### 返回值

int errno: 错误代码

其他成员类似 FileListResult.Entry

## 上传准备：获取文件哈希值

```cs
UploadHelper.GetFileProperty(string path) : FileProperty
```

在使用其他上传函数之前，需要先调用此函数，计算分块和秒传所需的分块哈希值等信息。

### 参数

string path: 本地文件全路径

### 返回值

记录文件哈希值的对象

## 极速秒传

```cs
Operation.RapidUpload(FileProperty prop, string path, Credential credential) : RapidUploadResult
```

通过提供文件的 MD5、CRC32、开头的 MD5、每个分块的 MD5，由服务器查询是否已经有此文件，从而实现“极速秒传”。失败则 errno = 404。

### 参数

FileProperty prop: 文件的哈希值信息

string path: 网盘上的文件保存路径

Credential credential: 身份信息

### 返回值

int errno: 错误代码

FileListResult.Entry info: 若成功上传，该文件的信息

## 初始化分块上传

```cs
Operation.InitUpload(FileProperty prop, string path, Credential credential) : InitUploadResult
```

在计算好一个文件的哈希信息后，文件的分块上传分为三个步骤：初始化，上传每个分块，以及提交。首先，调用此函数，上传文件的分块信息以及获得上传会话 ID。有可能服务器上已经有某些分块了，所以 block_list 不包括所有分块的编号。

使用方法可以参见 Operation.ChunkedUpload 的实现。

### 参数

FileProperty prop: 文件的哈希值信息

string path: 网盘上的文件保存路径

Credential credential: 身份信息

### 返回值

int errno: 错误代码

string uploadid: 上传会话的 ID

int[] block_list: 需要上传的分块编号

## 上传分块内容

```cs
Operation.UploadBlock(FileProperty prop, string path, InitUploadResult session, FileStream stream, int blockid, string host, Credential credential)
```

初始化完成后，由调用者打开一个能读取的 FileStream，这样每次上传一个分块时可以重用。

### 参数

FileProperty prop: 文件的哈希值信息

string path: 网盘上的文件保存路径

InitUploadResult session: 初始化得到的结果

FileStream stream: 供读取的文件流

int blockid: 块的序号

string host: 上传服务器的地址，可以用 GetUploadServers 函数获取

Credential credential: 身份信息

### 返回值

由 bool success 表示是否成功

## 提交分块上传

```cs
Operation.CommitUpload(FileProperty prop, string path, InitUploadResult session, Credential credential) : CommitUploadResult
```

上传完成所有要求的分块之后，调用此函数提交。返回新建文件的信息。如果文件名已存在，会自动重命名。

### 参数

FileProperty prop: 文件的哈希值信息

string path: 网盘上的文件保存路径

InitUploadResult session: 初始化得到的结果

Credential credential: 身份信息

### 返回值

int errno: 错误代码

其他成员类似 FileListResult.Entry

## 获得上传服务器列表

```cs
Operation.GetUploadServers(Credential credential) : GetUploadServersResult
```

### 参数

Credential credential: 身份信息

### 返回值

string[] servers: 服务器域名列表

# 分享

## 公开 / 私密分享

```cs
Operation.Share(string[] pathlist, Credential credential, string pwd = null) : ShareResult
```

### 参数

string[] pathlist: 要分享的文件（夹）列表

Credential credential: 身份信息

string pwd: 公开分享时，传入 null；私密分享时，传入 4 个字母的字符串，包括数字或大小写字母。

### 返回值

int errno: 错误代码

string link: 分享链接

string shorturl: 分享的短链接

## 转存他人的分享文件

```cs
Operation.Transfer(string url, string path, Credential credential, string pwd = null) : TransferResult
```

### 参数

string url: 分享页的地址

string path: 要保存到的文件夹

Credential credential: 身份信息

string pwd: 对于公开分享，传入 null；私密分享时，传入分享的密码。

### 返回值

int errno: 错误代码

TransferResult.Extra.Entry[] extra.list: 转存文件（夹）的信息

extra.list.from: 来自分享的路径

extra.list.to: 保存到的全路径

# 离线下载

可以提交链接，由百度网盘下载指定的文件，保存到网盘中。如果之前已经有人下载过相同的文件，可以实现“秒传”。支持的链接有 HTTP / HTTPS、magnet、ED2K，以及 BT种子。

*注意：BT 种子（.torrent 文件需要先上传到网盘当中）。*

## 获取离线下载任务列表

```cs
Operation.GetOfflineList(Credential credential) : OfflineListResult
```

### 参数

Credential credential: 身份信息

### 返回值

OfflineListResult.Entry[] tasks: 离线下载任务的数组

long tasks.create_time: 任务创建时间

string save_path: 保存到的全路径

string source_url: 源地址

long task_id: 下载任务的序号

string task_name: 显示的任务名称

long file_size: 所有文件的大小之和

long finished_size: 完成下载的大小

int status: 任务状态，0 表示完成，其他未知

## 查询磁力链 / BT 种子信息

```cs
Operation.QueryLinkFiles(string link, Credential credential) : QueryLinkResult
```

### 参数

string link: 磁力链，或 BT 种子在网盘上的全路径

Credential credential: 身份信息

### 返回值

string sha1: 如果是 BT 种子，稍后需要传入这个字符串

QueryLinkResult.Entry[] files: 种子中包含的文件（夹）信息

string files.file_name: 文件（夹）名称

long size: 文件大小

## 添加离线下载任务

```cs
Operation.AddOfflineTask(string link, string savepath, Credential credential, int[] selected = null, string sha1 = "") : AddOfflineTaskResult
```

### 参数

string link: 链接，或 BT 种子在网盘上的全路径

string savepath: 要保存到的文件夹

Credential credential: 身份信息

int[] selected: 对于磁力链或 BT 种子，选中项目在 QueryLinkFiles 返回值当中的下标，从 1 开始计数

string sha1: 如果是 BT 种子，传入 QueryLinkFiles 返回值中的 sha1

### 返回值

int rapid_download: 是否秒传

long task_id: 任务序号

## 取消 / 删除 / 清空离线下载任务

```cs
Operation.CancelOfflineTask(long taskid, Credential credential) : Result
Operation.DeleteOfflineTask(long taskid, Credential credential) : Result
Operation.ClearOfflineTask(Credential credential) : Result
```

未完成的任务（status != 0）可以取消（Cancel），完成的任务可以删除（Delete）。清空列表（Clear）只删除所有已完成的离线任务。

### 参数

long taskid: AddOfflineTask 返回的任务序号

Credential credential: 身份信息

### 返回值

由 bool success 表示是否成功

