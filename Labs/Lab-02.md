---
lab:
    title: 'Cosmos DB로 MongoDB 마이그레이션'
    module: '모듈 2: Cosmos DB로 MongoDB 워크로드 마이그레이션'
---
 
# 랩: Cosmos DB로 MongoDB 마이그레이션

## 예상 소요 시간

90분

## 시나리오

이 랩에서는 기존 MongoDB 데이터베이스를 가져와 Cosmos DB로 마이그레이션합니다. 마이그레이션을 수행할 때는 Azure Database Migration Service를 사용합니다. 또한 MongoDB 데이터베이스를 사용하는 기존 애플리케이션이 Cosmos DB 데이터베이스에 대신 연결하도록 다시 구성하는 방법도 알아봅니다.

이 랩의 내용은 일련의 IoT 디바이스에서 온도 데이터를 캡처하는 예제 시스템을 기준으로 합니다. 온도는 타임스탬프와 함께 MongoDB 데이터베이스에 기록됩니다. 각 디바이스에는 고유 ID가 지정되어 있습니다. 이 랩에서 실행할 MongoDB 애플리케이션은 이러한 디바이스를 시뮬레이트하고 데이터베이스에 데이터를 저장합니다. 그리고 사용자가 각 디바이스 관련 통계 정보를 쿼리할 수 있는 두 번째 애플리케이션도 사용할 것입니다. MongoDB에서 Cosmos DB로 데이터베이스를 마이그레이션한 후에는 두 애플리케이션이 모두 Cosmos DB에 연결하도록 구성하여 계속 올바르게 작동하는지 확인합니다.

랩을 실행할 때는 Azure Cloud Shell과 Azure Portal을 사용합니다.

## 목표

이 랩에서는 다음 작업을 수행합니다.

* 마이그레이션 프로젝트 만들기
* 마이그레이션의 원본과 대상 정의
* 마이그레이션 수행
* 마이그레이션 확인

## 참고

이 랩의 전체 지침은 https://github.com/MicrosoftLearning/DP-060KO-Migrating-your-Database-to-Cosmos-DB/blob/master/Labs/Lab%202%20-%20Migrate%20MongoDB%20Workloads%20to%20Cosmos%20DB.md 에서 제공됩니다.

이 랩의 모든 파일은 https://github.com/MicrosoftLearning/DP-060KO-Migrating-your-Database-to-Cosmos-DB 에 있습니다.

학생은 랩을 수행하기 전에 랩 파일 복사본을 다운로드해야 합니다.

## 랩 연습

* 마이그레이션 프로젝트 만들기
* 원본과 대상 정의
* 마이그레이션 수행
* 마이그레이션 확인

## 랩 복습

이 랩에서는 기존 MongoDB 데이터베이스를 가져와 Cosmos DB로 마이그레이션했습니다. Azure Database Migration Service를 사용하여 이 마이그레이션을 수행했습니다. 또한 MongoDB 데이터베이스를 사용하는 기존 애플리케이션이 Cosmos DB 데이터베이스에 대신 연결하도록 다시 구성하는 방법도 알아보았습니다.
