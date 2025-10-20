USE [SimulaEmprestimo];
GO

-- Habilitar inserção explícita na coluna identity
SET IDENTITY_INSERT dbo.PRODUTO ON;

-- Inserir produtos na tabela PRODUTO
INSERT INTO dbo.PRODUTO (CO_PRODUTO, NO_PRODUTO, PC_TAXA_JUROS, NU_MINIMO_MESES, NU_MAXIMO_MESES, VR_MINIMO, VR_MAXIMO) 
VALUES 
(1, 'Produto 1', 0.017900000, 0, 24, 200.00, 10000.00),
(2, 'Produto 2', 0.017500000, 25, 48, 10001.00, 100000.00),
(3, 'Produto 3', 0.018200000, 49, 96, 100000.01, 1000000.00),
(4, 'Produto 4', 0.015100000, 96, NULL, 1000000.01, NULL);
GO

-- Desabilitar IDENTITY_INSERT após a inserção
SET IDENTITY_INSERT dbo.PRODUTO OFF;
GO

-- Verificar os dados inseridos com formatação para visualizar as casas decimais
SELECT 
    CO_PRODUTO,
    NO_PRODUTO,
    PC_TAXA_JUROS,
    CAST(PC_TAXA_JUROS AS DECIMAL(10,8)) AS TAXA_FORMATADA,
    FORMAT(PC_TAXA_JUROS, '0.00000000') AS TAXA_STRING,
    NU_MINIMO_MESES,
    NU_MAXIMO_MESES,
    VR_MINIMO,
    VR_MAXIMO
FROM dbo.PRODUTO
ORDER BY CO_PRODUTO;

-- Verificar os dados inseridos
SELECT * FROM dbo.PRODUTO;
GO

-- Apagar todos os registros
-- DELETE FROM dbo.PRODUTO;
-- GO

