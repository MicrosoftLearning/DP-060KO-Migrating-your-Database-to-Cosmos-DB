---
lab:
    title: 'Cosmos DB로 Cassandra 마이그레이션'
    module: '모듈 3: Cosmos DB로 Cassandra 워크로드 마이그레이션'
---
 
# 랩: Cosmos DB로 Cassandra 마이그레이션

## 예상 소요 시간

90분

## 시나리오

이 랩에서는 데이터 집합 2개를 Cassandra에서 Cosmos DB로 마이그레이션합니다. 이 마이그레이션에서는 두 가지 방법으로 데이터를 이동합니다. 먼저 Cassandra에서 데이터를 내보낸 다음 CQLSH COPY 명령을 사용하여 데이터베이스를 Cosmos DB로 가져옵니다. 그리고 나서 Spark를 사용하여 데이터를 마이그레이션합니다. 그 후에는 원래 Cassandra 데이터베이스에 저장되어 있었던 데이터를 쿼리하는 애플리케이션을 실행하고 해당 애플리케이션이 Cosmos DB에 연결하도록 다시 구성하여 마이그레이션이 정상적으로 완료되었는지 확인합니다. 다시 구성한 애플리케이션을 실행한 결과는 다시 구성하기 전과 같아야 합니다.

이 랩에서는 전자 상거래 시스템 관련 시나리오를 진행합니다. 이 시스템에서는 고객이 상품을 주문할 수 있습니다. 고객 및 주문 세부 정보는 Cassandra 데이터베이스에 기록됩니다. 그리고 특정 애플리케이션이 특정 고객의 주문 목록, 특정 제품 주문, 다양한 집계(예: 접수된 주문 수 등)와 같은 요약을 생성합니다.

랩을 실행할 때는 Azure Cloud Shell과 Azure Portal을 사용합니다.

## 목표

이 랩에서는 다음 작업을 수행합니다.

* 스키마 내보내기
* CQLSH COPY를 사용하여 데이터 이동
* Spark를 사용하여 데이터 이동
* 마이그레이션 확인

## 참고

이 랩의 전체 지침은 https://github.com/MicrosoftLearning/DP-060T00A-Migrating-your-Database-to-Cosmos-DB/blob/master/Labs/Lab%203%20-%20Migrate%20Cassandra%20Workloads%20to%20Cosmos%20DB.md에서 제공됩니다.

이 랩의 모든 파일은 https://github.com/MicrosoftLearning/DP-160T00A-Migrating-your-Database-to-Cosmos-DB에 있습니다.

학생은 랩을 수행하기 전에 랩 파일 복사본을 다운로드해야 합니다.

## 랩 연습

* 스키마 내보내기
* CQLSH COPY를 사용하여 데이터 이동
* Spark를 사용하여 데이터 이동
* 마이그레이션 확인

## 랩 복습

이 랩에서는 데이터 집합 2개를 Cassandra에서 Cosmos DB로 마이그레이션했습니다. 이 마이그레이션에서는 두 가지 방법으로 데이터를 이동했습니다. 먼저 Cassandra에서 데이터를 내보낸 다음 CQLSH COPY 명령을 사용하여 데이터베이스를 Cosmos DB로 가져왔습니다. 그리고 나서 Spark를 사용하여 데이터를 마이그레이션했습니다. 그 후에는 원래 Cassandra 데이터베이스에 저장되어 있었던 데이터를 쿼리하는 애플리케이션을 실행하고 해당 애플리케이션이 Cosmos DB에 연결하도록 다시 구성하여 마이그레이션이 정상적으로 완료되었는지 확인했습니다.
