# 0.0.O 
# premier push


erDiagram
    %% CLIENT → ÉVÉNEMENT → PRODUITS / SERVICES
    CLIENT ||--o{ EVENT    : "a"
    EVENT  ||--o{ PRODUCT  : "inclut"
    EVENT  ||--o{ SERVICE  : "planifie"
    CLIENT ||--o{ PRODUCT  : "possède (optionnel)"
    CLIENT ||--o{ SERVICE  : "possède (optionnel)"

    %% SERVICE_PROTO ↔ PRODUCT_PROTO (relation de besoin)
    SERVICE_PROTO }o--o{ PRODUCT_PROTO : "requiert"

    %% Catégories et prototypes
    PRODUCT_CATEGORY ||--o{ PRODUCT_PROTO    : "définit"
    PRODUCT_PROTO    ||--o{ PRODUCT          : "instancie"
    SERVICE_CATEGORY ||--o{ SERVICE_PROTO    : "définit"
    SERVICE_PROTO    ||--o{ SERVICE          : "instancie"

    %% SCHÉMA DES TABLES
    CLIENT {
        UUID     Id PK
        string   DisplayName
        string   Email
        string   Phone
        json     ExtraData
        bool     IsDeleted
        datetime CreatedAt
        datetime LastUpdated
    }

    EVENT {
        UUID     Id PK
        UUID     ClientId FK
        string   Title
        text     Notes
        datetime StartAt
        datetime EndAt
        json     ExtraData
        enum     Status
        bool     IsDeleted
        datetime CreatedAt
        datetime LastUpdated
    }

    PRODUCT_CATEGORY {
        UUID   Id PK
        string Name
        json   FieldSchema
        bool   IsDeleted
        datetime CreatedAt
        datetime LastUpdated
    }

    PRODUCT_PROTO {
        UUID   Id PK
        UUID   CategoryId FK
        string Name
        decimal UnitPrice
        json   DefaultExtra
        json   CategoryData
        bool   IsDeleted
        datetime CreatedAt
        datetime LastUpdated
    }

    PRODUCT {
        UUID   Id PK
        UUID   ClientId FK
        UUID   EventId FK
        UUID   ProtoId  FK
        int    Quantity
        decimal UnitPrice
        json   ExtraData
        json   CategoryData
        bool   IsDeleted
        datetime CreatedAt
        datetime LastUpdated
    }

    SERVICE_CATEGORY {
        UUID   Id PK
        string Name
        json   FieldSchema
        bool   IsDeleted
        datetime CreatedAt
        datetime LastUpdated
    }

    SERVICE_PROTO {
        UUID   Id PK
        UUID   CategoryId FK
        string Name
        decimal BasePrice
        json   DefaultExtra
        json   CategoryData
        bool   IsDeleted
        datetime CreatedAt
        datetime LastUpdated
    }

    SERVICE {
        UUID     Id PK
        UUID     ClientId FK
        UUID     EventId  FK
        UUID     ProtoId  FK
        decimal  Price
        json     ExtraData
        json     CategoryData
        datetime PerformedAt
        bool     IsDeleted
        datetime CreatedAt
        datetime LastUpdated
    }
