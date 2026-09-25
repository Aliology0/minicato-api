using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlMostashar.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTypedClientRequestDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UploadedByClientId",
                table: "CaseDocuments",
                newName: "UploadedByUserId");

            migrationBuilder.RenameIndex(
                name: "IX_CaseDocuments_UploadedByClientId",
                table: "CaseDocuments",
                newName: "IX_CaseDocuments_UploadedByUserId");

            migrationBuilder.CreateTable(
                name: "CompanyFormationRequestDetails",
                columns: table => new
                {
                    ClientRequestId = table.Column<int>(type: "int", nullable: false),
                    CompanyType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BusinessActivity = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CapitalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    FoundersCount = table.Column<int>(type: "int", nullable: false),
                    HasPowerOfAttorney = table.Column<bool>(type: "bit", nullable: false),
                    ProposedCompanyName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyFormationRequestDetails", x => x.ClientRequestId);
                    table.ForeignKey(
                        name: "FK_CompanyFormationRequestDetails_ClientRequests_ClientRequestId",
                        column: x => x.ClientRequestId,
                        principalTable: "ClientRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConsultationRequestDetails",
                columns: table => new
                {
                    ClientRequestId = table.Column<int>(type: "int", nullable: false),
                    LegalBranch = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PreferredAppointmentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CommunicationMethod = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ConsultationSummary = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsultationRequestDetails", x => x.ClientRequestId);
                    table.ForeignKey(
                        name: "FK_ConsultationRequestDetails_ClientRequests_ClientRequestId",
                        column: x => x.ClientRequestId,
                        principalTable: "ClientRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContractRequestDetails",
                columns: table => new
                {
                    ClientRequestId = table.Column<int>(type: "int", nullable: false),
                    ContractType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ContractRequestType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Language = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PagesCount = table.Column<int>(type: "int", nullable: true),
                    AllowedRevisions = table.Column<int>(type: "int", nullable: true),
                    DeliveryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OtherPartyName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractRequestDetails", x => x.ClientRequestId);
                    table.ForeignKey(
                        name: "FK_ContractRequestDetails_ClientRequests_ClientRequestId",
                        column: x => x.ClientRequestId,
                        principalTable: "ClientRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GenericRequestDetails",
                columns: table => new
                {
                    ClientRequestId = table.Column<int>(type: "int", nullable: false),
                    LegalBranch = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Summary = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    DesiredOutcome = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ImportantDates = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GenericRequestDetails", x => x.ClientRequestId);
                    table.ForeignKey(
                        name: "FK_GenericRequestDetails_ClientRequests_ClientRequestId",
                        column: x => x.ClientRequestId,
                        principalTable: "ClientRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LawsuitRequestDetails",
                columns: table => new
                {
                    ClientRequestId = table.Column<int>(type: "int", nullable: false),
                    LegalBranch = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsCaseAlreadyFiled = table.Column<bool>(type: "bit", nullable: false),
                    CourtName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CaseNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    NextHearingDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LawsuitStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ClientRole = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    OpponentName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LawsuitRequestDetails", x => x.ClientRequestId);
                    table.ForeignKey(
                        name: "FK_LawsuitRequestDetails_ClientRequests_ClientRequestId",
                        column: x => x.ClientRequestId,
                        principalTable: "ClientRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Backfill typed rows while retaining RequestDetailsJson as a legacy safety net.
            migrationBuilder.Sql("""
                INSERT INTO ConsultationRequestDetails (ClientRequestId, LegalBranch, PreferredAppointmentDate, CommunicationMethod, ConsultationSummary)
                SELECT Id, COALESCE(JSON_VALUE(RequestDetailsJson, '$.legalBranch'), 'Other'), TRY_CONVERT(datetime2, JSON_VALUE(RequestDetailsJson, '$.preferredAppointmentDate')), COALESCE(JSON_VALUE(RequestDetailsJson, '$.communicationMethod'), 'Text'), JSON_VALUE(RequestDetailsJson, '$.consultationSummary')
                FROM ClientRequests WHERE ServiceType = 'Consultation' AND ISJSON(RequestDetailsJson) = 1;

                INSERT INTO ContractRequestDetails (ClientRequestId, ContractType, ContractRequestType, Language, PagesCount, AllowedRevisions, DeliveryDate, OtherPartyName)
                SELECT Id, COALESCE(JSON_VALUE(RequestDetailsJson, '$.contractType'), 'Other'), COALESCE(JSON_VALUE(RequestDetailsJson, '$.contractRequestType'), 'Review'), COALESCE(JSON_VALUE(RequestDetailsJson, '$.language'), 'Arabic'), TRY_CONVERT(int, JSON_VALUE(RequestDetailsJson, '$.pagesCount')), TRY_CONVERT(int, JSON_VALUE(RequestDetailsJson, '$.allowedRevisions')), TRY_CONVERT(datetime2, JSON_VALUE(RequestDetailsJson, '$.deliveryDate')), JSON_VALUE(RequestDetailsJson, '$.otherPartyName')
                FROM ClientRequests WHERE ServiceType = 'Contract' AND ISJSON(RequestDetailsJson) = 1;

                INSERT INTO LawsuitRequestDetails (ClientRequestId, LegalBranch, IsCaseAlreadyFiled, CourtName, CaseNumber, NextHearingDate, LawsuitStatus, ClientRole, OpponentName)
                SELECT Id, COALESCE(JSON_VALUE(RequestDetailsJson, '$.legalBranch'), 'Other'), CASE JSON_VALUE(RequestDetailsJson, '$.isCaseAlreadyFiled') WHEN 'true' THEN 1 ELSE 0 END, JSON_VALUE(RequestDetailsJson, '$.courtName'), JSON_VALUE(RequestDetailsJson, '$.caseNumber'), TRY_CONVERT(datetime2, JSON_VALUE(RequestDetailsJson, '$.nextHearingDate')), JSON_VALUE(RequestDetailsJson, '$.lawsuitStatus'), JSON_VALUE(RequestDetailsJson, '$.clientRole'), JSON_VALUE(RequestDetailsJson, '$.opponentName')
                FROM ClientRequests WHERE ServiceType = 'Lawsuit' AND ISJSON(RequestDetailsJson) = 1;

                INSERT INTO CompanyFormationRequestDetails (ClientRequestId, CompanyType, BusinessActivity, CapitalAmount, FoundersCount, HasPowerOfAttorney, ProposedCompanyName)
                SELECT Id, COALESCE(JSON_VALUE(RequestDetailsJson, '$.companyType'), 'Other'), COALESCE(JSON_VALUE(RequestDetailsJson, '$.businessActivity'), 'Unknown'), TRY_CONVERT(decimal(18,2), JSON_VALUE(RequestDetailsJson, '$.capitalAmount')), COALESCE(TRY_CONVERT(int, JSON_VALUE(RequestDetailsJson, '$.foundersCount')), 1), CASE JSON_VALUE(RequestDetailsJson, '$.hasPowerOfAttorney') WHEN 'true' THEN 1 ELSE 0 END, JSON_VALUE(RequestDetailsJson, '$.proposedCompanyName')
                FROM ClientRequests WHERE ServiceType = 'CompanyFormation' AND ISJSON(RequestDetailsJson) = 1;

                INSERT INTO GenericRequestDetails (ClientRequestId, LegalBranch, Summary, DesiredOutcome, ImportantDates)
                SELECT Id, JSON_VALUE(RequestDetailsJson, '$.legalBranch'), JSON_VALUE(RequestDetailsJson, '$.summary'), JSON_VALUE(RequestDetailsJson, '$.desiredOutcome'), JSON_VALUE(RequestDetailsJson, '$.importantDates')
                FROM ClientRequests WHERE ServiceType NOT IN ('Consultation','Contract','Lawsuit','CompanyFormation') AND ISJSON(RequestDetailsJson) = 1;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_CompanyFormationRequestDetails_CompanyType",
                table: "CompanyFormationRequestDetails",
                column: "CompanyType");

            migrationBuilder.CreateIndex(
                name: "IX_ConsultationRequestDetails_LegalBranch",
                table: "ConsultationRequestDetails",
                column: "LegalBranch");

            migrationBuilder.CreateIndex(
                name: "IX_ContractRequestDetails_ContractType",
                table: "ContractRequestDetails",
                column: "ContractType");

            migrationBuilder.CreateIndex(
                name: "IX_GenericRequestDetails_LegalBranch",
                table: "GenericRequestDetails",
                column: "LegalBranch");

            migrationBuilder.CreateIndex(
                name: "IX_LawsuitRequestDetails_CaseNumber",
                table: "LawsuitRequestDetails",
                column: "CaseNumber");

            migrationBuilder.CreateIndex(
                name: "IX_LawsuitRequestDetails_CourtName",
                table: "LawsuitRequestDetails",
                column: "CourtName");

            migrationBuilder.CreateIndex(
                name: "IX_LawsuitRequestDetails_LegalBranch",
                table: "LawsuitRequestDetails",
                column: "LegalBranch");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompanyFormationRequestDetails");

            migrationBuilder.DropTable(
                name: "ConsultationRequestDetails");

            migrationBuilder.DropTable(
                name: "ContractRequestDetails");

            migrationBuilder.DropTable(
                name: "GenericRequestDetails");

            migrationBuilder.DropTable(
                name: "LawsuitRequestDetails");

            migrationBuilder.RenameColumn(
                name: "UploadedByUserId",
                table: "CaseDocuments",
                newName: "UploadedByClientId");

            migrationBuilder.RenameIndex(
                name: "IX_CaseDocuments_UploadedByUserId",
                table: "CaseDocuments",
                newName: "IX_CaseDocuments_UploadedByClientId");

        }
    }
}
