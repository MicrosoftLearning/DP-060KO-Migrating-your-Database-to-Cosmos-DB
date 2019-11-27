---
lab:
    title: 'Cosmos DB로 MongoDB 워크로드 마이그레이션'
    module: '모듈 2: Cosmos DB로 MongoDB 워크로드 마이그레이션'
---
 
- [랩 2: Cosmos DB로 MongoDB 워크로드 마이그레이션](#lab-2-migrate-mongdb-workloads-to-cosmos-db)
  - [연습 1: 설정](#exercise-1-setup)
    - [태스크 1: 리소스 그룹 및 가상 네트워크 만들기](#task-1-create-a-resource-group-and-virtual-network)
    - [태스크 2: MongoDB 데이터베이스 서버 만들기](#task-2-create-a-mongodb-database-server)
    - [태스크 3: MongoDB 데이터베이스 구성](#task-3-configure-the-mongodb-database)
  - [연습 2: MongoDB 데이터베이스에 데이터 입력/데이터베이스 쿼리](#exercise-2-populate-and-query-the-mongodb-database)
    - [태스크 1: MongoDB 데이터베이스에 데이터를 입력하는 데 사용할 앱 빌드/실행](#task-1-build-and-run-an-app-to-populate-the-mongodb-database)
    - [태스크 2: MongoDB 데이터베이스를 쿼리하는 데 사용할 두 번째 앱 빌드/실행](#task-2-build-and-run-another-app-to-query-the-mongodb-database)
  - [연습 3: Cosmos DB로 MongoDB 데이터베이스 마이그레이션](#exercise-3-migrate-the-mongodb-database-to-cosmos-db)
    - [태스크 1: Cosmos 계정 및 데이터베이스 만들기](#task-1-create-a-cosmos-account-and-database)
    - [태스크 2: Database Migration Service 만들기](#task-2-create-the-database-migration-service)
    - [태스크 3: 새 마이그레이션 프로젝트 만들기/실행](#task-3-create-and-run-a-new-migration-project)
    - [태스크 4: 마이그레이션이 정상적으로 완료되었는지 확인](#task-4-verify-that-migration-was-successful)
  - [연습 4: Cosmos DB를 사용하도록 기존 애플리케이션 다시 구성/실행](#exercise-4-reconfigure-and-run-existing-applications-to-use-cosmos-db)
    - [태스크 1: MongoDB 집계를 지원하도록 설정](#task-1-enable-mongodb-aggregation-support)
    - [태스크 2: DeviceDataQuery 애플리케이션 다시 구성](#task-2-reconfigure-the-devicedataquery-application)
  - [연습 5: 정리](#exercise-5-clean-up)

# 랩 2: Cosmos DB로 MongoDB 워크로드 마이그레이션

이 랩에서는 기존 MongoDB 데이터베이스를 가져와 Cosmos DB로 마이그레이션합니다. 마이그레이션을 수행할 때는 Azure Database Migration Service를 사용합니다. 또한 MongoDB 데이터베이스를 사용하는 기존 애플리케이션이 Cosmos DB 데이터베이스에 대신 연결하도록 다시 구성하는 방법도 알아봅니다.

이 랩의 내용은 일련의 IoT 디바이스에서 온도 데이터를 캡처하는 예제 시스템을 기준으로 합니다. 온도는 타임스탬프와 함께 MongoDB 데이터베이스에 기록됩니다. 각 디바이스에는 고유 ID가 지정되어 있습니다. 이 랩에서 실행할 MongoDB 애플리케이션은 이러한 디바이스를 시뮬레이트하고 데이터베이스에 데이터를 저장합니다. 그리고 사용자가 각 디바이스 관련 통계 정보를 쿼리할 수 있는 두 번째 애플리케이션도 사용할 것입니다. MongoDB에서 Cosmos DB로 데이터베이스를 마이그레이션한 후에는 두 애플리케이션이 모두 Cosmos DB에 연결하도록 구성하여 계속 올바르게 작동하는지 확인합니다.

랩을 실행할 때는 Azure Cloud Shell과 Azure Portal을 사용합니다.

## 연습 1: 설정

첫 번째 연습에서는 온도 디바이스에서 캡처한 데이터를 저장할 MongoDB 데이터베이스를 만듭니다.

### 태스크 1: 리소스 그룹 및 가상 네트워크 만들기

1. 인터넷 브라우저에서 https://portal.azure.com으로 이동하여 로그인합니다.
2. Azure Portal에서 **리소스 그룹**, **+추가**를 차례로 클릭합니다.
3. **리소스 그룹 만들기** 페이지에서 다음 세부 정보를 입력하고 **검토 + 만들기**를 클릭합니다.

    | 속성  | 값  |
    |---|---|
    | 구독 | *\<사용자의 구독 이름\>* |
    | 리소스 그룹 | mongodbrg |
    | 지역 | 가장 가까운 위치 선택 |

4. **만들기**를 클릭하고 리소스 그룹이 만들어질 때까지 기다립니다.
5. Azure Portal의 왼쪽 창에서 **+ 리소스 만들기**를 클릭합니다.
6. **새로 만들기** 페이지의 **Marketplace 검색** 상자에 **가상 네트워크**를 입력한 후에 Enter 키를 누릅니다.
7. **가상 네트워크** 페이지에서 **만들기**를 클릭합니다.
8. **가상 네트워크 만들기** 페이지에서 다음 세부 정보를 입력하고 **만들기**를 클릭합니다.

    | 속성  | 값  |
    |---|---|
    | 이름 | databasevnet |
    | 주소 공간 | 10.0.0.0/24 |
    | 구독 | *\<사용자의 구독 이름\>* |
    | 리소스 그룹 | mongodbrg |
    | 지역 | 리소스 그룹용으로 선택한 것과 같은 위치 선택 |
    | 서브넷 이름 | default |
    | 서브넷 주소 범위 | 10.0.0.0/28 |
    | DDos 보호 | 기본 |
    | 서비스 엔드포인트 | 사용 안 함 |
    | 방화벽 | 사용 안 함 |

9. 가상 네트워크가 생성될 때까지 기다린 후에 다음 태스크를 계속 진행합니다.

### 태스크 2: MongoDB 데이터베이스 서버 만들기

1. Azure Portal의 왼쪽 창에서 **+ 리소스 만들기**를 클릭합니다.
2. **Marketplace 검색** 상자에 **MongoDB Certified by Bitnami**를 입력하고 Enter 키를 누릅니다.
3. **MongoDB Certified by Bitnami** 페이지에서 **만들기**를 클릭합니다.
4. **가상 머신 만들기** 페이지에서 다음 세부 정보를 입력하고 **다음: 디스크 \>**를 클릭합니다.

    | 속성  | 값  |
    |---|---|
    | 구독 | *\<사용자의 구독 이름\>* |
    | 리소스 그룹 | mongodbrg |
    | 가상 머신 이름 | mongodbserver | 
    | 지역 | 리소스 그룹용으로 선택한 것과 같은 위치 선택 |
    | 가용성 옵션 | 인프라 중복이 필요하지 않습니다. |
    | 이미지 | MongoDB Certified by Bitnami |
    | 크기 | Standard A1 v2 |
    | 인증 유형 | 암호 |
    | 사용자 이름 | azureuser |
    | 암호 | Pa55w.rdPa55w.rd |
    | 암호 확인 | Pa55w.rdPa55w.rd |

5. **디스크** 페이지에서 기본 설정을 적용하고 **다음: 네트워킹 \>**을 클릭합니다.
6. **네트워킹** 페이지에서 다음 세부 정보를 입력하고 **다음: 관리 \>**를 클릭합니다.

    | 속성  | 값  |
    |---|---|
    | 가상 네트워크 | databasevnet |
    | 서브넷 | 기본값(10.0.0.0/28) |
    | 공용 IP | (신규) mongodbserver-ip |
    | NIC 네트워크 보안 그룹 | 고급 |
    ! 네트워크 보안 그룹 구성 | (신규) mongodbserver-nsg |
    | 가속화된 네트워킹 | 해제 |
    | 부하 분산 | 아니요 |

7. **관리** 페이지에서 기본 설정을 적용하고 **다음: 고급 \>**을 클릭합니다.
8. **고급** 페이지에서 기본 설정을 적용하고 **다음: 태그 \>**를 클릭합니다.
9. **태그** 페이지에서 기본 설정을 적용하고 **다음: 검토 + 만들기 \>**를 클릭합니다.
10. 유효성 검사 페이지에서 **만들기**를 클릭합니다.
11. 가상 머신이 배포될 때까지 기다린 후에 다음 과정을 계속 진행합니다.
12. Azure Portal의 왼쪽 창에서 **모든 리소스**를 클릭합니다.
13. **모든 리소스** 페이지에서 **mongodbserver-nsg**를 클릭합니다.
14. **mongodbserver-nsg** 페이지의 **설정**에서 **인바운드 보안 규칙**을 클릭합니다.
15. **mongodbserver-nsg - 인바운드 보안 규칙** 페이지에서 **+ 추가**를 클릭합니다.
16. **인바운드 보안 규칙 추가** 창에서 다음 세부 정보를 입력하고 **추가**를 클릭합니다.

    | 속성  | 값  |
    |---|---|
    | 원본 | 모두 |
    | 원본 포트 범위 | * |
    | 대상 | 모두 |
    | 대상 포트 범위 | 27017 |
    | 프로토콜 | 모두 |
    | 작업 | 허용 |
    | 우선 순위 | 1020 |
    | 이름 | Mongodb-port |
    | 설명 | 클라이언트가 MongoDB에 연결하는 데 사용하는 포트 |

### 태스크 3: MongoDB 데이터베이스 구성

1. Azure Portal의 왼쪽 창에서 **모든 리소스**를 클릭합니다.
2. **모든 리소스** 페이지에서 **mongodbserver-ip**를 클릭합니다.
3. **mongodbserver-ip** 페이지에 표시되는 **IP 주소**를 적어 둡니다.
4. Azure Portal 상단의 도구 모음에서 **Cloud Shell**을 클릭합니다.
5. **탑재된 스토리지가 없음** 메시지 상자가 표시되면 **스토리지 만들기**를 클릭합니다.
6. Cloud Shell이 시작되면 Cloud Shell 창 위의 드롭다운 목록에서 **Bash**를 선택합니다.
7. Cloud Shell에서 다음 명령을 입력하여 mongodbserver 가상 머신에 연결합니다. 여기서 *\<ip 주소\>*는 **mongodbserver-ip** IP 주소의 값으로 바꾸세요.

    ```bash
    ssh azureuser@<ip address>
    ```

8. 프롬프트에서 **yes**를 입력하여 연결을 계속 진행합니다.
9. 암호 **Pa55w.rdPa55w.rd**를 입력합니다.
10. 다음 명령을 입력하고 표시되는 루트 암호를 적어 둡니다.

    ```bash
    cat bitnami_credentials
    ```

11. 다음 명령을 실행하여 MongoDB 데이터베이스에 연결합니다. 여기서 *\<암호\>*는 이전 단계에서 표시된 루트 암호로 바꾸세요. XFS 파일 시스템 사용 관련 경고는 무시하면 됩니다.

    ```bash
    mongo -u root -p <password>
    ```

12. **>** 프롬프트에서 다음 명령을 실행합니다. 이러한 명령은 **DeviceData** 데이터베이스용으로 이름이 **deviceadmin**이고 암호가 **Pa55w.rd**인 새 사용자를 만듭니다. `db.shutdownserver();` 명령을 실행하면 오류가 몇 개 표시되지만 무시해도 됩니다.

    ```mongosh
    use DeviceData;
    db.createUser(
        {
            user: "deviceadmin",
            pwd: "Pa55w.rd",
            roles: [ { role: "readWrite", db: "DeviceData" } ]
        }
    );
    use admin;
    db.shutdownServer();
    exit;
    ```

13. 다음 명령을 실행하여 mongodb 서비스를 다시 시작합니다. 서비스가 오류 메시지 없이 다시 시작되고 포트 27017에서 수신 대기하는지 확인합니다.

    ```bash
    sudo /opt/bitnami/ctlscript.sh start
    ```

14. 다음 명령을 실행하여 이제 deviceadmin 사용자로 mongodb에 로그인할 수 있는지 확인합니다.

    ```bash
    mongo -u "deviceadmin" -p "Pa55w.rd" --authenticationDatabase DeviceData
    ```

15. **>** 프롬프트에서 다음 명령을 실행하여 mongo shell을 종료합니다.

    ```mongosh
    exit;
    ```

16. **bitnami@mongodbserver** 프롬프트에서 다음 명령을 입력하여 MongoDB 서버에서 연결을 끊고 Cloud Shell로 돌아옵니다.

    ```bash
    exit
    ```

## 연습 2: MongoDB 데이터베이스에 데이터 입력/데이터베이스 쿼리

지금까지 MongoDB 서버와 데이터베이스를 만들었습니다. 다음 단계에서는 이 데이터베이스에 데이터를 입력하고 데이터베이스를 쿼리할 수 있는 샘플 애플리케이션을 실행합니다.

### 태스크 1: MongoDB 데이터베이스에 데이터를 입력하는 데 사용할 앱 빌드/실행

1. Azure Cloud Shell에서 다음 명령을 실행하여 이 워크샵용 샘플 코드를 다운로드합니다.

    ```bash
    git clone https://github.com/MicrosoftLearning/DP-160T00A-Migrating-your-Database-to-Cosmos-DB migration-workshop-apps
    ```

2. **migration-workshop-apps/MongoDeviceDataCapture/MongoDeviceCapture** 폴더로 이동합니다.

    ```bash
    cd ~/migration-workshop-apps/MongoDeviceDataCapture/MongoDeviceDataCapture
    ```

3. **코드** 편집기를 사용하여 **TemperatureDevice.cs** 파일을 검사합니다.

    ```bash
    code TemperatureDevice.cs
    ```

    이 파일의 코드에 포함되어 있는 **TemperatureDevice** 클래스는 데이터를 캡처하여 MongoDB 데이터베이스에 저장하는 온도 디바이스를 시뮬레이트합니다. 그리고 .NET Framework용 MongoDB 라이브러리를 사용합니다. **TemperatureDevice** 생성자는 애플리케이션 구성 파일에 저장된 설정을 사용하여 데이터베이스에 연결합니다. **RecordTemperatures** 메서드는 판독값 하나를 생성하여 데이터베이스에 씁니다.

4. 코드 편집기를 닫고 **ThermometerReading.cs** 파일을 엽니다.

   ```bash
   code ThermometerReading.cs
   ```

    이 파일에는 애플리케이션이 데이터베이스에 저장하는 문서의 구조가 나와 있습니다. 각 문서에는 다음 필드가 포함됩니다.

    - 개체 ID. 각 문서를 고유하게 식별하기 위해 MongoDB에서 생성하는 "_id" 필드입니다.
    - 디바이스 ID. 각 디바이스에는 "Device" 접두사가 붙은 번호가 지정됩니다.
    - 디바이스가 기록한 온도
    - 온도가 기록된 날짜와 시간
  
5. 코드 편집기를 닫고 **App.config** 파일을 엽니다.

    ```bash
    code App.config
    ```

    이 파일에는 MongoDB 데이터베이스에 연결하는 데 필요한 설정이 포함되어 있습니다. **Address** 키의 값을 앞에서 적어 둔 MongoDB 서버의 IP 주소로 설정하고 파일을 저장한 후에 편집기를 닫습니다.

6. 다음 명령을 실행하여 애플리케이션을 다시 빌드합니다.

    ```bash
    dotnet build
    ````

7. 애플리케이션을 실행합니다.

    ```bash
    dotnet run
    ```

    이 애플리케이션은 동시에 실행되는 디바이스 100개를 시뮬레이트합니다. 애플리케이션을 몇 분 정도 실행한 후에 Enter 키를 눌러 실행을 중지합니다.

### 태스크 2: MongoDB 데이터베이스를 쿼리하는 데 사용할 두 번째 앱 빌드/실행

1. **migration-workshop-apps/MongoDeviceDataCapture/DeviceDataQuery** 폴더로 이동합니다.

    ```bash
    cd ~/migration-workshop-apps/MongoDeviceDataCapture/DeviceDataQuery
    ```

    이 폴더에는 각 디바이스에서 캡처한 데이터를 분석하는 데 사용할 수 있는 다른 애플리케이션이 포함되어 있습니다.

2. **코드** 편집기를 사용하여 **Program.cs** 파일을 검사합니다.

    ```bash
    code Program.cs
    ```

    이 애플리케이션은 파일 하단의 **ConnectToDatabase** 메서드를 사용하여 데이터베이스에 연결한 다음 사용자에게 디바이스 번호를 입력하라는 메시지를 표시합니다. 그리고 .NET Framework용 MongoDB 라이브러리를 사용해 집계 파이프라인을 만들고 실행합니다. 이 파이프라인은 지정된 디바이스의 다음 통계를 계산합니다.

    - 기록된 판독값 수
    - 기록된 평균 온도
    - 최저 판독값
    - 최고 판독값
    - 최신 판독값

3. 코드 편집기를 닫고 **App.config** 파일을 엽니다.

    ```bash
    code App.config
    ```

    이전 태스크에서와 같이 **Address** 키의 값을 앞에서 적어 둔 MongoDB 서버의 IP 주소로 설정하고 파일을 저장한 후에 편집기를 닫습니다.

4. 애플리케이션을 빌드하고 실행합니다.

    ```bash
    dotnet build
    dotnet run
    ```

5. **디바이스 번호 입력** 프롬프트에 0에서 99 사이의 값을 입력합니다. 애플리케이션이 데이터베이스를 쿼리하고 통계를 계산한 후에 결과를 표시합니다. **Q** 키를 눌러 애플리케이션을 종료합니다.

## 연습 3: Cosmos DB로 MongoDB 데이터베이스 마이그레이션

다음 단계에서는 MongoDB 데이터베이스를 가져온 다음 Cosmos DB로 전송합니다.

### 태스크 1: Cosmos 계정 및 데이터베이스 만들기

1. Azure Portal로 돌아옵니다.
2. 왼쪽 창에서 **+ 리소스 만들기**를 클릭합니다.
3. **새로 만들기** 페이지의 **Marketplace 검색** 상자에 ***Azure Cosmos DB**를 입력한 후에 Enter 키를 누릅니다.
4. **Azure Cosmos DB** 페이지에서 **만들기**를 클릭합니다.
5. **Azure Cosmos DB 계정 만들기** 페이지에서 다음 설정을 입력하고 **검토 + 만들기**를 클릭합니다.

    | 속성  | 값  |
    |---|---|
    | 구독 | 사용자의 구독 선택 |
    | 리소스 그룹 | mongodbrg |
    | 계정 이름 | mongodb*nnn*. 여기서 *nnn*에는 임의로 선택한 숫자를 입력하면 됩니다. |
    | API | Azure Cosmos DB for MongoDB API |
    | 위치 | MongoDB 서버 및 가상 네트워크에 사용한 것과 같은 위치 지정 |
    | 지리적 중복 | 사용 안 함 |
    | 다중 영역 쓰기 | 사용 안 함 |

6. 유효성 검사 페이지에서 **만들기**를 클릭하고 Cosmos DB 계정이 배포될 때까지 기다립니다.
7. 왼쪽 창에서 **Azure Cosmos DB**를 클릭합니다.
8. **Azure Cosmos DB** 페이지에서 Cosmos DB 계정(**mongodb*nnn***)을 클릭합니다.
9. **mongodb*nnn*** 페이지에서 **Data Explorer**를 클릭합니다.
10. **Data Explorer** 창에서 **새 컬렉션**을 클릭합니다.
11. **컬렉션 추가** 창에서 다음 설정을 지정하고 **확인**을 클릭합니다.

    | 속성  | 값  |
    |---|---|
    | 데이터베이스 ID | **새로 만들기**를 클릭하고 **DeviceData**를 입력합니다. |
    | 데이터베이스 처리량 프로비전 | 선택 |
    | 처리량 | 1000 |
    | 컬렉션 ID | Temperatures |
    | 공유 키 | deviceID |

### 태스크 2: Database Migration Service 만들기

1. 왼쪽 창에서 **모든 서비스**를 클릭합니다.
2. **모든 서비스** 페이지에서 **구독**을 클릭합니다.
3. **구독** 페이지에서 사용자의 구독을 클릭합니다.
4. 구독 페이지의 **설정**에서 **리소스 공급자**를 클릭합니다.
5. **이름으로 필터링** 상자에 **DataMigration**을 입력한 다음 **Microsoft.DataMigration**을 클릭합니다.
6. **등록**을 클릭하고 **상태**가 **등록됨**으로 변경될 때까지 기다립니다. 변경된 상태를 확인하려면 **새로 고침**을 클릭해야 할 수 있습니다.
7. 왼쪽 창에서 **+ 리소스 만들기**를 클릭합니다.
8. **새로 만들기** 페이지의 **Marketplace 검색** 상자에 **Azure Database Migration Service**를 입력한 후에 Enter 키를 누릅니다.
9. **Azure Database Migration Service** 페이지에서 **만들기**를 클릭합니다.
10. **Migration Service 만들기** 페이지에서 다음 설정을 입력하고 **만들기**를 클릭합니다.

    | 속성  | 값  |
    |---|---|
    | 서비스 이름 | MongoDBMigration |
    | 구독 | 사용자의 구독 선택 |
    | 리소스 그룹 선택 | mongodbrg |
    | 위치 | 이전에 사용했던 것과 같은 위치 선택 |
    | 가상 네트워크 | **가상 네트워크 선택 또는 만들기**를 클릭하고 **databasevnet/default**를 선택한 다음 **확인**을 클릭합니다. |
    | 가격 책정 계층 | Standard: vCore 1개 |

11. 계속하기 전에 서비스가 배포될 때까지 기다립니다. 이 작업은 몇 분 정도 걸립니다.

### 태스크 3: 새 마이그레이션 프로젝트 만들기/실행

1. 왼쪽 창에서 **리소스 그룹**을 클릭합니다.
2. **리소스 그룹** 창에서 **mongodbrg**를 클릭합니다.
3. **mongodbrg** 창에서 **MongoDBMigration**을 클릭합니다.
4. **MongoDBMigration** 페이지에서 **+ 새 마이그레이션 프로젝트**를 클릭합니다.
5. **새 마이그레이션 프로젝트** 페이지에서 다음 설정을 입력하고 **활동 만들기 및 실행**을 클릭합니다.

    | 속성  | 값  |
    |---|---|
    | 프로젝트 이름 | MigrateTemperatureData |
    | 원본 서버 유형 | MongoDB |
    | 대상 서버 유형 | Cosmos DB(MongoDB API) |
    | 활동 유형 선택 | 오프라인 데이터 마이그레이션 |

6. **마이그레이션 마법사**가 시작되면 **원본 세부 정보** 페이지에서 다음 세부 정보를 입력하고 **저장**을 클릭합니다.

    | 속성  | 값  |
    |---|---|
    | 모드 | 표준 모드 |
    | 원본 서버 이름 | 이전에 적어 둔 **mongodbserver-ip** IP 주소의 값 지정 |
    | 사용자 이름 | root |
    | 암호 | 앞에서 bitnami_credentials 파일을 확인하여 적어 둔 mongodbserver VM의 root 사용자 암호 입력 |
    | SSL 필요 | 비워 둠 |

7. **마이그레이션 정보** 페이지에서 다음 세부 정보를 입력하고 **저장**을 클릭합니다.

    | 속성  | 값  |
    |---|---|
    | 모드 | Cosmos DB 대상 선택 |
    | 구독 | 사용자의 구독 선택 |
    | Comos DB 이름 | mongodb*nnn* |
    | 연결 문자열 | Cosmos DB 계정용으로 생성된 연결 문자열 적용 |

8. **대상 데이터베이스에 매핑** 페이지에서 다음 세부 정보를 입력하고 **저장**을 클릭합니다.

    | 속성  | 값  |
    |---|---|
    | 원본 데이터베이스 | DeviceData |
    | 대상 데이터베이스 | DeviceData |
    | 처리량(RU/s) | 1000 |
    | 컬렉션 정리 | 확인란 선택 취소 |

9. **컬렉션 설정** 페이지에서 DeviceData 데이터베이스 옆의 드롭다운 화살표를 클릭하고 다음 세부 정보를 입력한 후에 **저장**을 클릭합니다.

    | 속성  | 값  |
    |---|---|
    | 이름 | Temperatures |
    | 대상 컬렉션 | Temperatures |
    | 처리량(RU/s) | 1000 |
    | 공유 키 | deviceID |
    | 고유 | 비워 둠 |

10. **마이그레이션 요약** 페이지의 **활동 이름** 필드에 **mongodb-migration**을 입력하고 **초기 데이터 복사 중에 RU 증가**를 선택한 후에 **마이그레이션 실행**을 클릭합니다.
11. **mongodb-migration** 페이지에서 마이그레이션이 완료될 때까지 30초마다 **새로 고침**을 클릭합니다. 마이그레이션된 문서 수를 적어 둡니다.

### 태스크 4: 마이그레이션이 정상적으로 완료되었는지 확인

1. 왼쪽 창에서 **Azure Cosmos DB**를 클릭합니다.
2. **Azure Cosmos DB** 페이지에서 **mongodb*nnn***을 클릭합니다.
3. **mongodb*nnn** 페이지에서 **Data Explorer**를 클릭합니다.
4. **Data Explorer** 창에서 **Temperatures** 데이터베이스를 확장하고 **문서**를 클릭합니다.
5. **문서** 창에서 문서 목록을 스크롤합니다. 각 문서의 ID(**_id**)와 공유 키(**/deviceID**)가 표시됩니다.
6. 아무 문서나 클릭합니다. 표시한 문서의 세부 정보가 나타납니다. 일반적인 문서의 형식은 다음과 같습니다.

    ```JSON
    {
	    "_id" : ObjectId("5ce8104bf56e8a04a2d0929a"),
	    "deviceID" : "Device 83",
	    "temperature" : 19.65268837271849,
	    "time" : 636943091952553500
    }
    ```

7. **문서 탐색기** 창의 도구 모음에서 **새 셸**을 클릭합니다.
8. **셸 1** 창의 **\>** 프롬프트에서 다음 명령을 입력하고 Enter 키를 누릅니다.

    ```mongosh
    db.Temperatures.count()
    ```

    이 명령은 Temperatures 컬렉션의 문서 수를 표시합니다. 이 문서 수는 마이그레이션 마법사가 보고한 수와 일치해야 합니다.

9. 다음 명령을 입력하고 Enter 키를 누릅니다.

    ```mongosh
    db.Temperatures.find({deviceID: "Device 99"})
    ```

    이 명령은 디바이스 99의 문서를 가져와서 표시합니다.

## 연습 4: Cosmos DB를 사용하도록 기존 애플리케이션 다시 구성/실행

마지막 단계에서는 기존 MongoDB 애플리케이션이 Cosmos DB에 연결하도록 다시 구성하고 이전처럼 작동하는지 확인합니다. 이 프로세스에서는 애플리케이션이 데이터베이스에 연결하는 방식을 수정하되 애플리케이션의 논리는 그대로 유지해야 합니다.

### 태스크 1: MongoDB 집계를 지원하도록 설정

1. **mongodb*nnn*** 창의 **설정**에서 **미리 보기 기능**을 클릭합니다.
2. **집계 파이프라인** 옆의 **사용**을 클릭합니다. 집계 파이프라인이 사용하도록 설정되는 동안 기다립니다. 이 기능은 Cosmos DB 데이터베이스에 MongoDB 집계 및 그룹화 작업 지원을 추가합니다. 집계 파이프라인이 사용 가능한 상태가 되려면 몇 분 정도 걸릴 수 있습니다. 브라우저 창을 새로 고쳐 기능의 최신 상태를 확인하세요.

### 태스크 2: DeviceDataQuery 애플리케이션 다시 구성

1. **mongodb*nnn*** 창의 **설정**에서 **연결 문자열**을 클릭합니다.
2. **mongodb*nnn* 연결 문자열** 페이지에서 다음 설정을 적어 둡니다.

    - 호스트
    - 사용자 이름
    - 주 암호
  
3. Cloud Shell 창으로 돌아와서 **migration-workshop-apps/MongoDeviceDataCapture/DeviceDataQuery** 폴더로 이동합니다. 세션 시간이 초과된 경우 다시 연결합니다.

    ```bash
    cd ~/migration-workshop-apps/MongoDeviceDataCapture/DeviceDataQuery
    ```

4. 코드 편집기에서 App.config 파일을 엽니다.

    ```bash
    code App.config
    ```

5. 파일의 **MongoDB 설정** 섹션에서 기존 설정을 주석 처리합니다.
6. **Cosmos DB Mongo API 설정** 섹션의 설정에서 주석 처리를 제거한 후에 해당 설정의 값을 다음과 같이 설정합니다.

    | 설정  | 값  |
    |---|---|
    | 주소 | **mongodb*nnn* 연결 문자열** 페이지의 호스트 |
    | 사용자 이름 | **mongodb*nnn* 연결 문자열** 페이지의 사용자 이름 |
    | 암호 | **mongodb*nnn* 연결 문자열** 페이지의 주 암호 |

    완성된 파일은 다음과 같습니다.

    ```XML
    <?xml version="1.0" encoding="utf-8"?>
    <configuration>
        <appSettings>
            <add key="Database" value="DeviceData" />
            <add key="Collection" value="Temperatures" />

            <!-- Settings for MongoDB -->
            <!--add key="Address" value="168.63.99.236" />
            <add key="Port" value="27017" />
            <add key="Username" value="deviceadmin" />
            <add key="Password" value="Pa55w.rd" /-->
            <!-- End of settings for MongoDB -->

            <!-- Settings for CosmosDB Mongo API -->
            <add key="Address" value="mongodbnnn.documents.azure.com"/>
            <add key="Port" value="10255"/>
            <add key="Username" value="mongodbnnn"/>
            <add key="Password" value="xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx=="/>
            <!-- End of settings for CosmosDB Mongo API -->
        </appSettings>
    </configuration>
    ```

7. 파일을 저장한 다음 코드 편집기를 닫습니다.

8. 코드 편집기에서 Program.cs 파일을 엽니다.

    ```bash
    code Program.cs
    ```

9. 아래쪽의 **ConnectToDatabase** 메서드로 스크롤합니다.
10. MongoDB 연결용 자격 증명을 설정하는 줄을 주석 처리하고 Cosmos DB 연결용 자격 증명을 지정하는 문의 주석 처리를 제거합니다. 이렇게 수정한 코드는 다음과 같습니다.

    ```C#
    // MongoDB 데이터베이스에 연결
    MongoClient client = new MongoClient(new MongoClientSettings
    {
        Server = new MongoServerAddress(address, port),
        ServerSelectionTimeout = TimeSpan.FromSeconds(10),

        //
        // MongoDB용 자격 증명 설정
        //

        // 자격 증명 = MongoCredential.CreateCredential(database, azureLogin.UserName, azureLogin.SecurePassword),

        //
        // CosmosDB Mongo API용 자격 증명 설정
        //

        UseSsl = true,
        SslSettings = new SslSettings
        {
            EnabledSslProtocols = SslProtocols.Tls12
        },
        Credential = new MongoCredential("SCRAM-SHA-1", new MongoInternalIdentity(database, azureLogin.UserName), new PasswordEvidence(azureLogin.SecurePassword))

        // Mongo API 설정 끝 
    });

    ```

    이와 같이 변경해야 하는 이유는 원본 MongoDB 데이터베이스는 SSL 연결을 사용하지 않았기 때문입니다. Cosmos DB는 항상 SSL을 사용합니다.

11. 파일을 저장한 다음 코드 편집기를 닫습니다.
12. 애플리케이션을 다시 빌드하고 실행합니다.

    ```bash
    dotnet build
    dotnet run
    ```

13. **디바이스 번호 입력** 프롬프트에 0에서 99 사이의 디바이스 번호를 입력합니다. 애플리케이션이 이전과 똑같이 실행됩니다. 단, 이번에는 Cosmos DB 데이터베이스에 저장된 데이터를 사용합니다.
14. 다른 디바이스 번호로 애플리케이션을 테스트합니다. 테스트를 완료하려면 **Q**를 입력합니다.

이 연습에서는 MongoDB 데이터베이스를 Cosmos DB로 마이그레이션했으며 기존 MongoDB 애플리케이션이 Cosmos DB 데이터베이스에 연결하도록 다시 구성했습니다.

## 연습 5: 정리

1. Azure Portal로 돌아옵니다.
2. 왼쪽 창에서 **리소스 그룹**을 클릭합니다.
3. **리소스 그룹** 창에서 **mongodbrg**를 클릭합니다.
4. **리소스 그룹 삭제**를 클릭합니다.
5. **"mongodbrg"을(를) 삭제하시겠습니까?** 페이지의 **리소스 그룹 이름 입력** 상자에 **mongodbrg**를 입력하고 **삭제**를 클릭합니다.

---
© 2019 Microsoft Corporation. All rights reserved.

이 문서의 내용은 [Creative Commons Attribution 3.0 라이선스](https://creativecommons.org/licenses/by/3.0/legalcode)에 따라 제공되며 추가 약관이 적용될 수 있습니다. 상표, 로고, 이미지 등을 비롯하여 이 문서에 포함된 기타 모든 콘텐츠는 Creative Commons 라이선스 허여 범위 내에 포함되지 **않습니다**. 이 문서는 Microsoft 제품의 지적 재산에 대한 법적 권리를 제공하지 않습니다. 이 문서는 내부 참조용으로 복사하여 사용할 수 있습니다.

이 문서는 "있는 그대로" 제공됩니다. 이 문서에 표현된 정보와 견해는 URL 및 기타 인터넷 웹 사이트 참조를 포함하여 예고 없이 변경될 수 있습니다. 이러한 내용을 사용하는 경우의 위험은 사용자가 부담합니다. 일부 예제는 설명을 위해서만 제공되는 가상의 예이며, 실제 사람/사물과는 어떠한 연관도 없고 그러한 연관을 추정해서도 안 됩니다. Microsoft는 이 문서에서 제공하는 정보와 관련하여 명시적이나 묵시적인 어떠한 보증도 하지 않습니다.
