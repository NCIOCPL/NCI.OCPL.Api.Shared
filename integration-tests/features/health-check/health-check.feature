Feature: The library offers an ability to check the health of the Elasticsearch index.

  Background:
    * url apiHost


    Scenario: The Index is healthy

    Given path 'HealthCheck'
    When method GET
    Then status 200
    And match response == 'true'
