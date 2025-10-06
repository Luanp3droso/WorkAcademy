# 💼 WorkAcademy

A **WorkAcademy** é uma plataforma digital inspirada no LinkedIn e na Udemy, desenvolvida em **ASP.NET Core (C#)** com **Entity Framework**, focada em **inclusão social e qualificação profissional**.  
O sistema conecta **usuários**, **empresas** e **oportunidades de emprego**, além de oferecer **cursos com certificados digitais**.

---

## 🚀 Funcionalidades Principais

### 👤 Usuários
- Cadastro e login (com suporte a autenticação social via Google, Facebook e LinkedIn)  
- Criação e edição de perfil  
- Upload e exibição de certificados  
- Sistema de publicações e comentários  
- Envio e recebimento de solicitações de conexão (amizades)  
- Página de perfil pública (com visual inspirado no Facebook)

### 🏢 Empresas
- Cadastro de empresas e painel exclusivo  
- Criação e gerenciamento de vagas de emprego  
- Criação e publicação de cursos (como na Udemy)  
- Visualização de candidaturas  
- Painel administrativo completo

### 🧑‍💻 Administradores
- Painel de controle com estatísticas e cards interativos  
- Canal de denúncias 🚨  
- Gerenciamento de usuários, empresas, vagas e cursos  
- Monitoramento de publicações e conteúdos inapropriados

---

## 🧱 Tecnologias Utilizadas

| Categoria | Tecnologias |
|------------|--------------|
| **Linguagem / Framework** | C#, ASP.NET Core MVC, Entity Framework Core |
| **Banco de Dados** | SQL Server (LocalDB) |
| **Frontend** | HTML5, CSS3, Bootstrap, Razor Views, JavaScript |
| **Autenticação** | Identity + Login Social (Google, Facebook, LinkedIn) |
| **Deploy / Infraestrutura** | IIS / Azure (planejado) |
| **Padrões e Boas Práticas** | MVC, Repository Pattern, Responsividade, Acessibilidade |

---

## 📂 Estrutura do Projeto

WorkAcademy/
│
├── Controllers/ # Lógica de controle (Usuário, Empresa, Cursos, Vagas, Admin)
├── Models/ # Modelos de dados (Usuario, Empresa, Curso, Vaga, Certificado, etc.)
├── Views/ # Páginas Razor (Home, Perfil, Cursos, Admin, etc.)
├── Services/ # Serviços de apoio (Notificações, Currículos, Perfis)
├── Data/ # Contexto do banco (ApplicationDbContext + Migrations)
├── wwwroot/ # Arquivos estáticos (CSS, JS, imagens e uploads)
└── appsettings.json # Configurações e Connection String


---

## ⚙️ Como Executar Localmente

### 🔧 Pré-requisitos
- [Visual Studio 2022](https://visualstudio.microsoft.com/) ou [VS Code](https://code.visualstudio.com/)
- [.NET SDK 9.0+](https://dotnet.microsoft.com/download)
- [SQL Server](https://www.microsoft.com/pt-br/sql-server/sql-server-downloads) ou LocalDB

### ▶️ Passos para rodar
1. Clone o repositório:
   ```bash
   git clone https://github.com/Luanp3droso/WorkAcademy.git

2. Acesse a pasta:

cd WorkAcademy

3. Configure a connection string no arquivo appsettings.json

4. dotnet ef database update

5. dotnet run

6. https://localhost:5001

🧾 Funcionalidades em Desenvolvimento

🗨️ Chat em tempo real entre usuários e empresas

🪪 Download automático de certificados PDF

🌐 Acessibilidade ampliada (visual e auditiva)

🧭 Dashboard com estatísticas de desempenho

📈 Sistema de avaliação e progresso em cursos

🎯 Objetivo do Projeto

Promover a inclusão social e digital por meio da educação e do trabalho, conectando pessoas, empresas e oportunidades em um único ambiente.
O WorkAcademy foi desenvolvido como Trabalho de Conclusão de Curso (TCC) do curso de Sistemas de Informação (AEDB).

👨‍💻 Autores

Luan Pedroso

📧 Email - luan3droso@gmail.com

🌐 LinkedIn - https://www.linkedin.com/in/luan-pedroso-002517187/

💻 Desenvolvedor Full Stack

Lucas Silva

📧 Email - Lucassilva13771@gmail.com

🌐 LinkedIn - https://www.linkedin.com/in/lucas-silva-555169226/

💻 Desenvolvedor Full Stack

🧩 Licença

Este projeto é distribuído sob a licença MIT.
Você é livre para usar, modificar e distribuir, desde que mantenha os créditos originais.