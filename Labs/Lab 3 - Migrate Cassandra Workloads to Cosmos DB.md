---
lab:
    title: 'Cosmos DB로 Cassandra 워크로드 마이그레이션'
    module: '모듈 3: Cosmos DB로 Cassandra 워크로드 마이그레이션'
---
 
- [랩 3: Cosmos DB로 Cassandra 워크로드 마이그레이션](#lab-3-migrate-cassandra-workloads-to-cosmos-db)
  - [연습 1: 설정](#exercise-1-setup)
    - [태스크 1: 리소스 그룹 및 가상 네트워크 만들기](#task-1-create-a-resource-group-and-virtual-network)
    - [태스크 2: Cassandra 데이터베이스 서버 만들기](#task-2-create-a-cassandra-database-server)
    - [태스크 3: Cassandra 데이터베이스에 데이터 입력](#task-3-populate-the-cassandra-database)
  - [연습 2: CQLSH COPY 명령을 사용하여 Cassandra에서 Cosmos DB로 데이터 마이그레이션](#exercise-2-migrate-data-from-cassandra-to-cosmos-db-using-the-cqlsh-copy-command)
    - [태스크 1: Cosmos 계정 및 데이터베이스 만들기](#task-1-create-a-cosmos-account-and-database)
    - [태스크 2: Cassandra 데이터베이스에서 데이터 내보내기](#task-2-export-the-data-from-the-cassandra-database)
    - [태스크 3: Cosmos DB로 데이터 가져오기](#task-3-import-the-data-to-cosmos-db)
    - [태스크 4: 데이터 마이그레이션이 정상적으로 완료되었는지 확인](#task-4-verify-that-data-migration-was-successful)
    - [태스크 5: 정리](#task-5-clean-up)
  - [연습 3: Spark를 사용하여 Cassandra에서 Cosmos DB로 데이터 마이그레이션](#exercise-3-migrate-data-from-cassandra-to-cosmos-db-using-spark)
    - [태스크 1: Spark 클러스터 만들기](#task-1-create-a-spark-cluster)
    - [태스크 2: 데이터 마이그레이션용 Notebook 만들기](#task-2-create-a-notebook-for-migrating-data)
    - [태스크 3: Cosmos DB에 연결/테이블 만들기](#task-3-connect-to-cosmos-db-and-create-tables)
    - [태스크 4: Cassandra 데이터베이스에 연결/데이터 검색](#task-4-connect-to-the-cassandra-database-and-retrieve-data)
    - [태스크 5: Cosmos DB 테이블에 데이터 삽입/Notebook 실행](#task-5-insert-data-into-cosmos-db-tables-and-run-the-notebook)
    - [태스크 6: 데이터 마이그레이션이 정상적으로 완료되었는지 확인](#task-6-verify-that-data-migration-was-successful)
    - [태스크 7: 정리](#task-7-clean-up)

# 랩 3: Cosmos DB로 Cassandra 워크로드 마이그레이션

이 랩에서는 데이터 집합 2개를 Cassandra에서 Cosmos DB로 마이그레이션합니다. 이 마이그레이션에서는 두 가지 방법으로 데이터를 이동합니다. 먼저 Cassandra에서 데이터를 내보낸 다음 **CQLSH COPY** 명령을 사용하여 데이터베이스를 Cosmos DB로 가져옵니다. 그리고 나서 Spark를 사용하여 데이터를 마이그레이션합니다. 그 후에는 Cosmos DB 데이터베이스에 저장된 데이터에 대해 쿼리를 실행하여 마이그레이션이 정상적으로 완료되었는지 확인합니다. 

이 랩에서는 전자 상거래 시스템 관련 시나리오를 진행합니다. 이 시스템에서는 고객이 상품을 주문할 수 있습니다. 고객 및 주문 세부 정보는 Cassandra 데이터베이스에 기록됩니다. 

랩을 실행할 때는 Azure Cloud Shell과 Azure Portal을 사용합니다.

## 연습 1: 설정

첫 번째 연습에서는 고객 및 주문 데이터 저장용 Cassandra 데이터베이스를 만듭니다.

### 태스크 1: 리소스 그룹 및 가상 네트워크 만들기

1. 인터넷 브라우저에서 https://portal.azure.com 으로 이동하여 로그인합니다.
2. Azure Portal에서 **리소스 그룹**, **+추가**를 차례로 클릭합니다.
3. **리소스 그룹 만들기** 페이지에서 다음 세부 정보를 입력하고 **검토 + 만들기**를 클릭합니다.

    | 속성  | 값  |
    |---|---|
    | 구독 | *\<사용자의 구독 이름\>* |
    | 리소스 그룹 | cassandradbrg |
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
    | 리소스 그룹 | cassandradbrg |
    | 리소스 그룹 | mongodbrg |
    | 지역 | 리소스 그룹용으로 선택한 것과 같은 위치 선택 |
    | 서브넷 이름 | default |
    | 서브넷 주소 범위 | 10.0.0.0/28 |
    | DDos 보호 | 기본 |
    | 서비스 엔드포인트 | 사용 안 함 |
    | 방화벽 | 사용 안 함 |

9. 가상 네트워크가 생성될 때까지 기다린 후에 다음 태스크를 계속 진행합니다.

### 태스크 2: Cassandra 데이터베이스 서버 만들기

1. Azure Portal의 왼쪽 창에서 **+ 리소스 만들기**를 클릭합니다.
2. **Marketplace 검색** 상자에 **Cassandra Certified by Bitnami**를 입력하고 Enter 키를 누릅니다.
3. **Cassandra Certified by Bitnami** 페이지에서 **만들기**를 클릭합니다.
4. **가상 머신 만들기** 페이지에서 다음 세부 정보를 입력하고 **다음: 디스크 \>**를 클릭합니다.

    | 속성  | 값  |
    |---|---|
    | 구독 | *\<사용자의 구독 이름\>* |
    | 리소스 그룹 | cassandradbrg |
    | 가상 머신 이름 | cassandraserver |
    | 지역 | 리소스 그룹용으로 선택한 것과 같은 위치 선택 |
    | 가용성 옵션 | 인프라 중복이 필요하지 않습니다. |
    | 이미지 | Cassandra Certified by Bitnami |
    | 크기 | Standard D2 v2 |
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
    | 공용 IP | (신규) cassandraserver-ip |
    | NIC 네트워크 보안 그룹 | 고급 |
    ! 네트워크 보안 그룹 구성 | (신규) cassandraserver-nsg |
    | 가속화된 네트워킹 | 해제 |
    | 부하 분산 | 아니요 |

7. **관리** 페이지에서 기본 설정을 적용하고 **다음: 고급 \>**을 클릭합니다.
8. **고급** 페이지에서 기본 설정을 적용하고 **다음: 태그 \>**를 클릭합니다.
9. **태그** 페이지에서 기본 설정을 적용하고 **다음: 검토 + 만들기 \>**를 클릭합니다.
10. 유효성 검사 페이지에서 **만들기**를 클릭합니다.
11. 가상 머신이 배포될 때까지 기다린 후에 다음 과정을 계속 진행합니다.
12. Azure Portal의 왼쪽 창에서 **모든 리소스**를 클릭합니다.
13. **모든 리소스** 페이지에서 **cassandraserver-nsg**를 클릭합니다.
14. **cassandraserver-nsg** 페이지의 **설정**에서 **인바운드 보안 규칙**을 클릭합니다.
15. **cassandraserver-nsg - 인바운드 보안 규칙** 페이지에서 **+ 추가**를 클릭합니다.
15. **인바운드 보안 규칙 추가** 창에서 다음 세부 정보를 입력하고 **추가**를 클릭합니다.

    | 속성  | 값  |
    |---|---|
    | 원본 | 모두 |
    | 원본 포트 범위 | * |
    | 대상 | 모두 |
    | 대상 포트 범위 | 9042 |
    | 프로토콜 | 모두 |
    | 작업 | 허용 |
    | 우선 순위 | 1020 |
    | 이름 | Cassandra-port |
    | 설명 | 클라이언트가 Cassandra에 연결하는 데 사용하는 포트 |

### 태스크 3: Cassandra 데이터베이스에 데이터 입력

1. Azure Portal의 왼쪽 창에서 **모든 리소스**를 클릭합니다.
2. **모든 리소스** 페이지에서 **cassandraserver-ip**를 클릭합니다.
3. **cassandraserver-ip** 페이지에 표시되는 **IP 주소**를 적어 둡니다.
4. Azure Portal 상단의 도구 모음에서 **Cloud Shell**을 클릭합니다.
5. **탑재된 스토리지가 없음** 메시지 상자가 표시되면 **스토리지 만들기**를 클릭합니다.
6. Cloud Shell이 시작되면 Cloud Shell 창 위의 드롭다운 목록에서 **Bash**를 선택합니다.
7. 랩 2를 수행하지 않은 경우 Cloud Shell에서 다음 명령을 실행하여 이 워크샵용 샘플 코드 및 데이터를 다운로드합니다.

    ```bash
    git clone https://github.com/MicrosoftLearning/DP-160T00A-Migrating-your-Database-to-Cosmos-DB migration-workshop-apps
    ```

8. **migration-workshop-apps/Cassandra** 폴더로 이동합니다.

    ```bash
    cd ~/migration-workshop-apps/Cassandra
    ```

9. 다음 명령을 입력하여 **cassandraserver** 가상 머신에 설치 스크립트 및 데이터를 복사합니다. 여기서 *\<ip 주소\>*는 **cassandraserver-ip** IP 주소의 값으로 바꾸세요.

    ```bash
    scp *.* azureuser@<ip address>:~
    ```

10. 프롬프트에서 **yes**를 입력하여 연결을 계속 진행합니다.
11. **암호** 프롬프트에서 암호 **Pa55w.rdPa55w.rd**를 입력합니다.
12. 다음 명령을 입력하여 **cassandraserver** 가상 머신에 연결합니다. **cassandraserver** 가상 머신의 IP 주소를 지정합니다.

    ```bash
    ssh azureuser@<ip address>
    ```

13. **암호** 프롬프트에서 암호 **Pa55w.rdPa55w.rd**를 입력합니다.
14. 다음 명령을 실행하여 Cassandra 데이터베이스에 연결하고 이 랩에 필요한 테이블을 만든 후에 데이터를 입력합니다.

    ```bash
    bash upload.sh
    ```

    위의 스크립트는 키스페이스 2개(**customerinfo** 및 **orderinfo**)를 만듭니다. 그리고 **customerinfo** 키스페이스에는 **customerdetails** 테이블을, **orderinfo** 키스페이스에는 테이블 2개(**orderdetails** 및 **orderline**)를 만듭니다.

15. 다음 명령을 실행하고 이 파일의 기본 암호를 적어 둡니다.

    ```bash
    cat bitnami_credentials
    ```

16. 가상 머신을 설정할 때 생성한 기본 Cassandra 사용자의 이름인 **cassandra** 사용자로 Cassandra Query Shell을 시작합니다. 여기서 *\<암호\>*는 이전 단계에서 적어 둔 기본 암호로 바꾸세요.

    ```bash
    cqlsh -u cassandra -p <password>
    ```

17. **cassandra@cqlsh** 프롬프트에서 다음 명령을 실행합니다. 이 명령은 **customerinfo.customerdetails** 테이블의 첫 100개 행을 표시합니다.

    ```cqlsh
    select *
    from customerinfo.customerdetails
    limit 100;
    ```

    데이터는 **stateprovince** 열을 기준으로 클러스터링된 다음 **customerid**를 기준으로 정렬됩니다. 이와 같이 데이터가 그룹화되므로 애플리케이션이 같은 지역의 모든 고객을 빠르게 찾을 수 있습니다.

18. 다음 명령을 실행합니다. 이 명령은 **orderinfo.orderdetails** 테이블의 첫 100개 행을 표시합니다.

    ```cqlsh
    select *
    from orderinfo.orderdetails
    limit 100;
    ```

    **orderinfo.orderdetails** 테이블에는 각 고객이 주문한 목록이 포함되어 있습니다. 기록되는 데이터에는 주문 날짜와 주문 가격이 포함됩니다. 데이터는 **customerid** 열을 기준으로 클러스터링되므로 애플리케이션이 지정된 고객의 모든 주문을 빠르게 찾을 수 있습니다.

19. 다음 명령을 실행합니다. 이 명령은 **orderinfo.orderline** 테이블의 첫 100개 행을 표시합니다.

    ```cqlsh
    select *
    from orderinfo.orderline
    limit 100;
    ```

    이 테이블에는 각 주문의 품목이 포함됩니다. 데이터는 **orderid** 열을 기준으로 클러스터링된 다음 **orderline**을 기준으로 정렬됩니다.

20. Cassandra Query Shell을 끝냅니다.

    ```cqlsh
    exit;
    ```

21. **bitnami@cassandraserver** 프롬프트에서 다음 명령을 입력하여 Cassandra 서버에서 연결을 끊고 Cloud Shell로 돌아옵니다.

    ```bash
    exit
    ```

## 연습 2: CQLSH COPY 명령을 사용하여 Cassandra에서 Cosmos DB로 데이터 마이그레이션 

지금까지 Cassandra 데이터베이스를 만들고 데이터를 입력했습니다. 이 연습에서는 Cassandra API를 사용하여 Cosmos DB 계정을 만든 다음 Cassandra 데이터베이스에서 Cosmos DB 계정의 데이터베이스로 데이터를 마이그레이션합니다.

### 태스크 1: Cosmos 계정 및 데이터베이스 만들기

1. Azure Portal로 돌아옵니다.
2. 왼쪽 창에서 **+ 리소스 만들기**를 클릭합니다.
3. **새로 만들기** 페이지의 **Marketplace 검색** 상자에 ***Azure Cosmos DB**를 입력한 후에 Enter 키를 누릅니다.
4. **Azure Cosmos DB** 페이지에서 **만들기**를 클릭합니다.
5. **Azure Cosmos DB 계정 만들기** 페이지에서 다음 설정을 입력하고 **검토 + 만들기**를 클릭합니다.

    | 속성  | 값  |
    |---|---|
    | 구독 | 사용자의 구독 선택 |
    | 리소스 그룹 | cassandradbrg |
    | 계정 이름 | cassandra*nnn*. 여기서 *nnn*에는 임의로 선택한 숫자를 입력하면 됩니다. |
    | API | Cassandra |
    | 위치 | Cassandra 서버 및 가상 네트워크에 사용한 것과 같은 위치 지정 |
    | 지리적 중복 | 사용 안 함 |
    | 다중 영역 쓰기 | 사용 안 함 |

6. 유효성 검사 페이지에서 **만들기**를 클릭하고 Cosmos DB 계정이 배포될 때까지 기다립니다.
7. 왼쪽 창에서 **Azure Cosmos DB**를 클릭합니다.
8. **Azure Cosmos DB** 페이지에서 Cosmos DB 계정(**cassandra*nnn***)을 클릭합니다.
9. **cassandra*nnn*** 페이지에서 **Data Explorer**를 클릭합니다.
10. **Data Explorer** 창에서 **새 테이블**을 클릭합니다.
11. **테이블 추가** 창에서 다음 설정을 지정하고 **확인**을 클릭합니다.

    | 속성  | 값  |
    |---|---|
    | 키스페이스 이름 | **새로 만들기**를 클릭하고 **customerinfo**를 입력합니다. |
    | 프로비전 키스페이스 처리량 | 선택 취소 |
    | tableId 입력 | customerdetails |
    | *CREATE TABLE* 상자 | (customerid int, firstname text, lastname text, email text, stateprovince text, PRIMARY KEY ((stateprovince), customerid)) |
    | 처리량 | 10000 |

12. **Data Explorer** 창에서 **새 테이블**을 클릭합니다.
13. **테이블 추가** 창에서 다음 설정을 지정하고 **확인**을 클릭합니다.

    | 속성  | 값  |
    |---|---|
    | 키스페이스 이름 | **새로 만들기**를 클릭하고 **orderinfo**를 입력합니다. |
    | 프로비전 키스페이스 처리량 | 선택 취소 |
    | tableId 입력 | orderdetails |
    | *CREATE TABLE* 상자 | (orderid int, customerid int, orderdate date, ordervalue decimal, PRIMARY KEY ((customerid), orderdate, orderid)) |
    | 처리량 | 10000 |

14. **Data Explorer** 창에서 **새 테이블**을 클릭합니다.
15. **테이블 추가** 창에서 다음 설정을 지정하고 **확인**을 클릭합니다.

    | 속성  | 값  |
    |---|---|
    | 키스페이스 이름 | **기존 항목 사용**을 클릭한 다음 **orderinfo**를 선택합니다. |
    | tableId 입력 | orderline |
    | *CREATE TABLE* 상자 | (orderid int, orderline int, productname text, quantity smallint, orderlinecost decimal, PRIMARY KEY ((orderid), productname, orderline)) |
    | 처리량 | 10000 |

### 태스크 2: Cassandra 데이터베이스에서 데이터 내보내기

1. Cloud Shell로 돌아옵니다.
2. 다음 명령을 실행하여 cassandra 서버에 연결합니다: 여기서 *\<ip address\>*은 가상 머신의 IP 주소로 바꾸세요. 메시지가 표시되면 암호 **Pa55w.rdPa55w.rd**를 입력합니다.

    ```bash
    ssh azureuser@<ip address>
    ```

3. Cassandra Query Shell을 시작합니다. **bitnami_credentials** 파일에서 암호를 지정합니다.

    ```bash
    cqlsh -u cassandra -p <password>
    ```

4. **cassandra@cqlsh** 프롬프트에서 다음 명령을 실행합니다. 이 명령은 **customerinfo.customerdetails** 테이블의 데이터를 다운로드하여 **customerdata** 파일에 씁니다. 이 명령에서 내보내는 행 수는 19119개입니다.

    ```cqlsh
    copy customerinfo.customerdetails
    to 'customerdata';
    ```

5. 다음 명령을 실행하여 **orderinfo.orderdetails** 테이블의 데이터를 **orderdata** 파일로 내보냅니다. 이 명령에서 내보내는 행 수는 31465개입니다.

   ```cqlsh
    copy orderinfo.orderdetails
    to 'orderdata';
    ```

6. 다음 명령을 실행하여 **orderinfo.orderline** 테이블의 데이터를 **orderline** 파일로 내보냅니다. 이 명령에서 내보내는 행 수는 121317개입니다.

   ```cqlsh
    copy orderinfo.orderline
    to 'orderline';
    ```

7. Cassandra Query Shell을 닫습니다.

    ```cqlsh
    exit;
    ```

### 태스크 3: Cosmos DB로 데이터 가져오기

1. Azure Portal에서 Cosmos DB 계정으로 다시 전환합니다.
2. **설정**에서 **연결 문자열**을 클릭하고 다음 항목을 적어 둡니다.

   - 접점
   - 포트
   - 사용자 이름
   - 주 암호

3. Cassandra 서버로 돌아와 Cassandra Query Shell을 시작합니다. 이번에는 Cosmos DB 계정에 연결합니다. **cqlsh** 명령의 인수는 방금 적어 둔 값으로 바꾸세요.

    ```bash
    export SSL_VERSION=TLSv1_2
    export SSL_VALIDATE=false

    cqlsh <contact point> <port> -u <username> -p <primary password> --ssl
    ```

    Cosmos DB에서는 SSL 연결을 사용해야 합니다.

4. **cqlsh** 프롬프트에서 다음 명령을 실행하여 이전에 Cassandra 데이터베이스에서 내보낸 데이터를 가져옵니다.

    ```cqlsh
    copy customerinfo.customerdetails from 'customerdata' with chunksize = 200;
    copy orderinfo.orderdetails from 'orderdata' with chunksize = 200;
    copy orderinfo.orderline from 'orderline' with chunksize = 100;
    ```

    위의 명령은 **customerdetails** 행 19119개, **orderdetails** 행 31465개, **orderline** 행 121317개를 가져옵니다. 이러한 명령의 실행이 실패하고 시간 제한 오류가 발생하면 다음의 두 가지 방법 중 하나로 문제를 해결할 수 있습니다.
    - Azure Portal에서 해당 테이블의 처리량을 늘립니다. 처리량 요금이 과도하게 발생하지 않도록 하려는 경우 나중에 처리량을 다수 줄이면 됩니다.
    - 각 복사 작업의 청크 크기를 줄입니다. 청크 크기를 줄이면 수집 속도가 느려지며 처리량을 늘리면 비용이 증가합니다.

### 태스크 4: 데이터 마이그레이션이 정상적으로 완료되었는지 확인

1. Azure Portal에서 Cosmos DB 계정으로 돌아와 **Data Explorer**를 클릭합니다.
2. **Data Explorer** 창에서 **customerinfo** 키스페이스와 **customerdetails** 테이블을 차례로 확장하고 **행**을 클릭합니다. 고객 집합이 나타나는지 확인합니다.
3. **새 절 추가**를 클릭합니다.
4. **필드** 상자에서 **stateprovince**를 선택하고 **값** 상자에 **Tasmania**를 입력합니다.
5. 도구 모음에서 **쿼리 실행**을 클릭합니다. 쿼리에서 행 106개가 반환되는지 확인합니다.
6. **Data Explorer** 창에서 **customerinfo** 키스페이스와 **customerdetails** 테이블을 차례로 확장하고 **행**을 클릭합니다.
7. **Data Explorer** 창에서 **orderinfo** 키스페이스와 **orderdetails** 테이블을 차례로 확장하고 **행**을 클릭합니다.
8. **새 절 추가**를 클릭합니다.
9. **필드** 상자에서 **customerid**를 선택하고 **값** 상자에 **13999**를 입력합니다.
10. 도구 모음에서 **쿼리 실행**을 클릭합니다. 쿼리에서 행 2개가 반환되는지 확인합니다. 첫 번째 행의 **orderid**(46899)를 적어 둡니다.
11. **Data Explorer** 창에서 **orderinfo** 키스페이스와 **orderline** 테이블을 차례로 확장하고 **행**을 클릭합니다.
12. **Data Explorer** 창의 **orderinfo** 키스페이스에서 **orderline** 테이블을 확장하고 **행**을 클릭합니다.
13. **새 절 추가**를 클릭합니다.
14. **필드** 상자에서 **orderid**를 선택하고 **값** 상자에 **46899**를 입력합니다.
15. 도구 모음에서 **쿼리 실행**을 클릭합니다. 쿼리가 행 1개를 반환하며 이 행에 주문한 제품이 **Road-550-W Yellow, 40**으로 표시되는지 확인합니다.

지금까지 CQLSH COPY 명령을 사용하여 Cassandra 데이터베이스를 Cosmos DB로 마이그레이션했습니다.

### 태스크 5: 정리

1. Cassandra 서버에서 실행 중인 Cassandra Query Shell로 다시 전환합니다. 셸은 Cosmos DB 계정에 아직 연결되어 있어야 합니다. 셸을 먼저 닫았다면 다음과 같이 다시 엽니다.

    ```bash
    export SSL_VERSION=TLSv1_2
    export SSL_VALIDATE=false

    cqlsh <contact point> <port> -u <username> -p <primary password> --ssl
    ```

2. Cassandra Query Shell에서 다음 명령을 실행하여 키스페이스와 테이블을 제거합니다.

    ```cqlsh
    drop keyspace customerinfo;
    drop keyspace orderinfo;
    exit;
    ```

## 연습 3: Spark를 사용하여 Cassandra에서 Cosmos DB로 데이터 마이그레이션

이 연습에서도 이전에 사용한 것과 같은 데이터를 마이그레이션합니다. 단, 이번에는 Azure Databricks Notebook에서 Spark를 사용합니다.

### 태스크 1: Spark 클러스터 만들기

1. Azure Portal의 왼쪽 창에서 **+ 리소스 만들기**를 클릭합니다.
2. **새로 만들기** 페이지의 **Marketplace 검색** 상자에 **Azure Databricks**를 입력한 후에 Enter 키를 누릅니다.
3. **Azure Databricks** 페이지에서 **만들기**를 클릭합니다.
4. **Azure Databricks Service** 페이지에서 다음 세부 정보를 입력하고 **만들기**를 클릭합니다.

    | 속성  | 값  |
    |---|---|
    | 작업 영역 이름 | CassandraMigration |
    | 구독 | *\<your-subscription\>* |
    | 리소스 그룹 | 기존 항목(cassandradbrg) 사용 |
    | 위치 | 리소스 그룹용으로 선택한 것과 같은 위치 선택 |
    | 가격 책정 계층 | Standard |
    | 가상 네트워크에 Azure Databricks 작업 영역 배포 | 아니요 |

5. Databricks Service가 배포될 때까지 기다립니다.
6. 왼쪽 창에서 **리소스 그룹**, **cassandradbrg**, **CassandraMigration** Databricks Service를 차례로 클릭합니다.
7. **CassandraMigration** 페이지에서 **작업 영역 시작**을 클릭합니다.
8. **Azure Databricks** 페이지의 **일반 작업**에서 **새 클러스터**를 클릭합니다.
9. **새 클러스터** 페이지에서 다음 설정을 입력하고 **클러스터 만들기**를 클릭합니다.

    | 속성  | 값  |
    |---|---|
    | 클러스터 이름 | MigrationCluster |
    | 클러스터 모드 | 표준 |
    | Databrick 런타임 버전 | 런타임: 5.3(Scala 2.11, Spark 2.4.0) |
    | Python 버전 | 3 |
    | 자동 크기 조정 사용 | 선택 |
    | 다음 시간 후 종료 | 60 |
    | 작업자 유형 | 기본 설정 적용 |
    | 드라이버 종류 | 작업자와 동일 |

10. 클러스터가 생성될 때까지 기다립니다. 클러스터가 준비되면 **MigrationCluster**의 상태가 **실행 중**으로 보고됩니다. 이 프로세스는 몇 분 정도 걸립니다.

### 태스크 2: 데이터 마이그레이션용 Notebook 만들기

1. **클러스터** 페이지 왼쪽의 창에서 **Azure Databricks**를 클릭합니다.
2. **Azure Databricks** 페이지의 **일반 작업**에서 **라이브러리 가져오기**를 클릭합니다.
3. **라이브러리 만들기** 페이지에서 다음 설정을 입력하고 **만들기**를 클릭합니다.

    | 속성  | 값  |
    |---|---|
    | 라이브러리 원본 | Maven |
    | 리포지토리 | 비워 둠 |
    | 좌표 | com.datastax.spark:spark-cassandra-connector_2.11:2.4.0 |
    | 제외 | 비워 둠 |

    이 파일에는 Spark에서 Cassandra에 연결하는 데 필요한 클래스가 포함되어 있습니다.

4. **실행 중인 클러스터의 상태** 섹션이 표시되면 **MigrationCluster** 행에서 **설치되지 않음** 옆의 확인란을 선택하고 **설치**를 클릭합니다.
5. 라이브러리 상태가 **설치됨**으로 변경될 때까지 기다린 후에 다음 과정을 계속 진행합니다.
6. 왼쪽 창에서 **Azure Databricks**를 클릭합니다.
7. **Azure Databricks** 페이지의 **일반 작업**에서 **라이브러리 가져오기**를 다시 클릭합니다.
8. **라이브러리 만들기** 페이지에서 다음 설정을 입력하고 **만들기**를 클릭합니다.

    | 속성  | 값  |
    |---|---|
    | 라이브러리 원본 | Maven |
    | 리포지토리 | 비워 둠 |
    | 좌표 | com.microsoft.azure.cosmosdb:azure-cosmos-cassandra-spark-helper:1.0.0 |
    | 제외 | 비워 둠 |

    이 파일에는 Spark에서 Cosmos DB에 연결하는 데 필요한 클래스가 포함되어 있습니다.

9. **실행 중인 클러스터의 상태** 섹션이 표시되면 **MigrationCluster** 행에서 **설치되지 않음** 옆의 확인란을 선택하고 **설치**를 클릭합니다.
10. 라이브러리 상태가 **설치됨**으로 변경될 때까지 기다린 후에 다음 과정을 계속 진행합니다.
11. 왼쪽 창에서 **Azure Databricks**를 클릭합니다.
12. **Azure Databricks** 페이지의 **일반 작업**에서 **새 Notebook**을 클릭합니다.
13. **Notebook 만들기** 페이지에서 다음 설정을 입력하고 **만들기**를 클릭합니다.

    | 속성  | 값  |
    |---|---|
    | 이름 | MigrateData |
    | 언어 | Scala |
    | 클러스터 | MigrationCluster |

### 태스크 3: Cosmos DB에 연결/테이블 만들기

1. Notebook의 첫 번째 셀에 다음 코드를 입력합니다.

    ```scala
    // 라이브러리 가져오기

    import org.apache.spark.sql.cassandra._
    import org.apache.spark.sql._
    import org.apache.spark._
    import com.datastax.spark.connector._
    import com.datastax.spark.connector.cql.CassandraConnector
    import com.microsoft.azure.cosmosdb.cassandra
    ```

    이 코드는 Spark에서 Cosmos DB 및 Cassandra에 연결하는 데 필요한 형식을 가져옵니다.

2. 셀 오른쪽의 도구 모음에서 드롭다운 화살표를 클릭하고 **아래에 셀 추가**를 클릭합니다.
3. 새 셀에 다음 코드를 입력합니다. 접점, 사용자 이름 및 주 암호는 이전 연습에서 기록해 둔 Cosmos DB 계정의 값으로 지정합니다.

    ```scala
    // Cosmos DB용 연결 매개 변수 구성

    val cosmosDBConf = new SparkConf()
        .set("spark.cassandra.connection.host", "<접점>")
        .set("spark.cassandra.connection.port", "10350")
        .set("spark.cassandra.connection.ssl.enabled", "true")
        .set("spark.cassandra.auth.username", "<사용자 이름>")
        .set("spark.cassandra.auth.password", "<주 암호>")
        .set("spark.cassandra.connection.factory",
            "com.microsoft.azure.cosmosdb.cassandra.CosmosDbConnectionFactory")
        .set("spark.cassandra.output.batch.size.rows", "1")
        .set("spark.cassandra.connection.connections_per_executor_max", "1")
        .set("spark.cassandra.output.concurrent.writes", "1")
        .set("spark.cassandra.concurrent.reads", "1")
        .set("spark.cassandra.output.batch.grouping.buffer.size", "1")
        .set("spark.cassandra.connection.keep_alive_ms", "600000000")
    ```

    이 코드는 Cosmos DB 계정에 연결하도록 Spark 세션 매개 변수를 설정합니다.

4. 현재 셀 아래에 다른 셀을 추가하고 다음 코드를 입력합니다.

    ```scala
    // 키스페이스 및 테이블 만들기

    val cosmosDBConnector = CassandraConnector(cosmosDBConf)

    cosmosDBConnector.withSessionDo(session => session.execute("CREATE KEYSPACE customerinfo WITH replication = {'class': 'SimpleStrategy', 'replication_factor': 1}"))
    cosmosDBConnector.withSessionDo(session => session.execute("CREATE TABLE customerinfo.customerdetails (customerid int, firstname text, lastname text, email text, stateprovince text, PRIMARY KEY ((stateprovince), customerid)) WITH cosmosdb_provisioned_throughput=10000"))

    cosmosDBConnector.withSessionDo(session => session.execute("CREATE KEYSPACE orderinfo WITH replication = {'class': 'SimpleStrategy', 'replication_factor': 1}"))
    cosmosDBConnector.withSessionDo(session => session.execute("CREATE TABLE orderinfo.orderdetails (orderid int, customerid int, orderdate date, ordervalue decimal, PRIMARY KEY ((customerid), orderdate, orderid)) WITH cosmosdb_provisioned_throughput=10000"))

    cosmosDBConnector.withSessionDo(session => session.execute("CREATE TABLE orderinfo.orderline (orderid int, orderline int, productname text, quantity smallint, orderlinecost decimal, PRIMARY KEY ((orderid), productname, orderline)) WITH cosmosdb_provisioned_throughput=10000"))
    ```

    이러한 문은 테이블과 함께 orderinfo 및 customerinfo 키스페이스를 다시 작성합니다. 각 테이블에는 처리량 10000RU/s가 프로비전됩니다.

### 태스크 4: Cassandra 데이터베이스에 연결/데이터 검색

1. Notebook에서 셀을 또 하나 추가하고 다음 코드를 입력합니다. 여기서 *\<ip address\>* 은 가상 머신의 IP 주소로 바꾸고 이전에 **bitnami_credentials** 파일에서 검색한 암호를 지정합니다.

    ```scala
    // 원본 Cassandra 데이터베이스용 연결 매개 변수 구성

    val cassandraDBConf = new SparkConf()
        .set("spark.cassandra.connection.host", "<ip address>")
        .set("spark.cassandra.connection.port", "9042")
        .set("spark.cassandra.connection.ssl.enabled", "false")
        .set("spark.cassandra.auth.username", "cassandra")
        .set("spark.cassandra.auth.password", "<password>")
        .set("spark.cassandra.connection.connections_per_executor_max", "10")
        .set("spark.cassandra.concurrent.reads", "512")
        .set("spark.cassandra.connection.keep_alive_ms", "600000000")
    ```

2. 다른 셀을 추가하고 다음 코드를 입력합니다.

    ```scala
    // 원본 데이터베이스에서 고객 및 주문 데이터 검색

    val cassandraDBConnector = CassandraConnector(cassandraDBConf)
    var cassandraSparkSession = SparkSession
        .builder()
        .config(cassandraDBConf)
        .getOrCreate()

    val customerDataframe = cassandraSparkSession
        .read
        .format("org.apache.spark.sql.cassandra")
        .options(Map( "table" -> "customerdetails", "keyspace" -> "customerinfo"))
        .load

    println("Read " + customerDataframe.count + " rows")

    val orderDetailsDataframe = cassandraSparkSession
        .read
        .format("org.apache.spark.sql.cassandra")
        .options(Map( "table" -> "orderdetails", "keyspace" -> "orderinfo"))
        .load

    println("Read " + orderDetailsDataframe.count + " rows")

    val orderLineDataframe = cassandraSparkSession
        .read
        .format("org.apache.spark.sql.cassandra")
        .options(Map( "table" -> "orderline", "keyspace" -> "orderinfo"))
        .load

    println("Read " + orderLineDataframe.count + " rows")
    ```

    이 코드 블록은 Cassandra 데이터베이스의 테이블에서 Spark DataFrame 개체로 데이터를 찾아옵니다. 코드는 각 테이블에서 읽은 행 수를 표시합니다.

### 태스크 5: Cosmos DB 테이블에 데이터 삽입/Notebook 실행

1. 마지막 셀을 추가하고 다음 코드를 입력합니다.

    ```scala
    // Cosmos DB에 고객 데이터 쓰기

    val cosmosDBSparkSession = SparkSession
        .builder()
        .config(cosmosDBConf)
        .getOrCreate()

    // Cosmos DB에서 기존 테이블에 연결
    var customerCopyDataframe = cosmosDBSparkSession
        .read
        .format("org.apache.spark.sql.cassandra")
        .options(Map( "table" -> "customerdetails", "keyspace" -> "customerinfo"))
        .load

    // Cassandra 데이터베이스의 결과를 DataFrame에 병합
    customerCopyDataframe = customerCopyDataframe.union(customerDataframe)

    // Cosmos DB에 결과 다시 쓰기
    customerCopyDataframe.write
        .format("org.apache.spark.sql.cassandra")
        .options(Map( "table" -> "customerdetails", "keyspace" -> "customerinfo"))
        .mode(org.apache.spark.sql.SaveMode.Append)
        .save()

    // 같은 전략을 사용하여 Cosmos DB에 주문 데이터 쓰기
    var orderDetailsCopyDataframe = cosmosDBSparkSession
        .read
        .format("org.apache.spark.sql.cassandra")
        .options(Map( "table" -> "orderdetails", "keyspace" -> "orderinfo"))
        .load

    orderDetailsCopyDataframe = orderDetailsCopyDataframe.union(orderDetailsDataframe)

    orderDetailsCopyDataframe.write
        .format("org.apache.spark.sql.cassandra")
        .options(Map( "table" -> "orderdetails", "keyspace" -> "orderinfo"))
        .mode(org.apache.spark.sql.SaveMode.Append)
        .save()

    var orderLineCopyDataframe = cosmosDBSparkSession
        .read
        .format("org.apache.spark.sql.cassandra")
        .options(Map( "table" -> "orderline", "keyspace" -> "orderinfo"))
        .load

    orderLineCopyDataframe = orderLineCopyDataframe.union(orderLineDataframe)

    orderLineCopyDataframe.write
        .format("org.apache.spark.sql.cassandra")
        .options(Map( "table" -> "orderline", "keyspace" -> "orderinfo"))
        .mode(org.apache.spark.sql.SaveMode.Append)
        .save()
    ```

    이 코드는 Cosmos DB 데이터베이스의 각 테이블용으로 다른 DataFrame을 만듭니다. 각 DataFrame은 처음에는 비어 있습니다. 그런 다음 **union** 함수를 사용하여 각 Cassandra 테이블에 해당하는 DataFrame에서 데이터를 추가합니다. 마지막으로 추가된 DataFrame을 Cosmos DB 테이블에 다시 씁니다.

    Spark에서 제공되는 매우 유용한 추상화인 DataFrame API는 대량의 데이터를 아주 빠르게 전송할 때 대단히 효율적인 구조입니다.

2. Notebook 상단의 도구 모음에서 **모두 실행**을 클릭합니다.  그러면 클러스터가 시작되고 있음을 나타내는 메시지가 표시됩니다. 클러스터가 준비되면 Notebook이 각 셀의 코드를 차례로 실행합니다. 그리고 각 셀 아래에는 추가 메시지가 표시됩니다. DataFrame을 읽고 쓰는 데이터 전송 작업은 Spark 작업으로 실행됩니다. 작업을 확장하면 진행률을 확인할 수 있습니다. 오류 메시지가 표시되지 않고 각 셀의 코드 실행이 정상적으로 완료되어야 합니다.

### 태스크 6: 데이터 마이그레이션이 정상적으로 완료되었는지 확인

1. Azure Portal에서 Cosmos DB 계정으로 돌아옵니다.
2. **Data Explorer**를 클릭합니다.
3. **Data Explorer** 창에서 **customerinfo** 키스페이스와 **customerdetails** 테이블을 차례로 확장하고 **행**을 클릭합니다. 첫 100개 행이 표시됩니다. **Data Explorer** 창에 키스페이스가 표시되지 않으면 **새로 고침**을 클릭하여 디스플레이를 업데이트합니다.
4. **orderinfo** 키스페이스와 **orderdetails** 테이블을 차례로 확장하고 **행**을 클릭합니다. 이 테이블의 경우에도 첫 100개 행이 표시됩니다.
5. 마지막으로 **orderline** 테이블을 확장하고 **행**을 클릭합니다. 이 테이블의 첫 100개 행이 나타나는지 확인합니다.

지금까지 Databricks Notebook에서 Spark를 사용하여 Cassandra 데이터베이스를 Cosmos DB로 마이그레이션했습니다.

### 태스크 7: 정리

1. Azure Portal의 왼쪽 창에서 **리소스 그룹**을 클릭합니다.
2. **리소스 그룹** 창에서 **cassandradbrg**를 클릭합니다.
3. **리소스 그룹 삭제**를 클릭합니다.
4. **"cassandradbrg"을(를) 삭제하시겠습니까?** 페이지의 **리소스 그룹 이름 입력** 상자에 **cassandradbrg**를 입력하고 **삭제**를 클릭합니다.

---
© 2019 Microsoft Corporation. All rights reserved.

이 문서의 내용은 [Creative Commons Attribution 3.0
라이선스](https://creativecommons.org/licenses/by/3.0/legalcode)에 따라 제공되며 추가 약관이 적용될 수 있습니다. 상표, 로고, 이미지 등을 비롯하여 이 문서에 포함된 기타 모든 콘텐츠는
Creative Commons 라이선스 허여 범위 내에 포함되지 **않습니다**. 이 문서는 Microsoft 제품의 지적 재산에 대한 법적 권리를 제공하지 않습니다. 이 문서는 내부 참조용으로 복사하여 사용할 수 있습니다.

이 문서는 "있는 그대로" 제공됩니다. 이 문서에 표현된 정보와 견해는 URL 및 기타 인터넷 웹 사이트 참조를 포함하여 예고 없이 변경될 수 있습니다. 이러한 내용을 사용하는 경우의 위험은 사용자가 부담합니다. 일부 예제는 설명을 위해서만 제공되는 가상의 예이며, 실제 사람/사물과는 어떠한 연관도 없고 그러한 연관을 추정해서도 안 됩니다. Microsoft는 이 문서에서 제공하는 정보와 관련하여 명시적이나 묵시적인 어떠한 보증도 하지 않습니다.
