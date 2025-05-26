using System;
using System.IO;
using System.Diagnostics;   // <<< ESSENCIAL para Process e ProcessStartInfo
using System.Windows.Forms;


namespace FirebirdSync
{
    public partial class Form1 : Form
    {
        private string pastaConfig;
        private string configGbakPath;
        private string configIpsPath;
        private string gbakPath;

        private string ipOrigem, caminhoOrigem, usuarioOrigem, senhaOrigem;
        private string ipDestino, caminhoDestino, usuarioDestino, senhaDestino;


        public Form1()
        {
            InitializeComponent();
            this.Icon = new Icon("favicon.ico");

            pastaConfig = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FirebirdSync");
            configGbakPath = Path.Combine(pastaConfig, "configGBAK.txt");
            configIpsPath = Path.Combine(pastaConfig, "configIPS.txt");

            Directory.CreateDirectory(pastaConfig);
            VerificarConfiguracoes();
        }

        private void VerificarConfiguracoes()
        {
            // Selecionar gbak.exe se não existir
            if (!File.Exists(configGbakPath))
            {
                using var ofd = new OpenFileDialog
                {
                    Title = "Selecione o gbak.exe do Firebird",
                    Filter = "Executável GBAK (gbak.exe)|gbak.exe"
                };

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    File.WriteAllText(configGbakPath, ofd.FileName);
                }
                else
                {
                    MessageBox.Show("É necessário selecionar o gbak.exe na primeira execução.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Environment.Exit(1);
                }
            }

            gbakPath = File.ReadAllText(configGbakPath).Trim();

            // Criar configIPS se não existir
            if (!File.Exists(configIpsPath))
            {
                var conteudo = 
                    "ipOrigem=localhost/3050\n" +
                    "caminhoOrigem=/firebird/data/VERSAOATUALIZADA.FDB\n" +
                    "usuarioOrigem=SYSDBA\n" +
                    "senhaOrigem=masterkey\n" +
                    "ipDestino=localhost/3051\n" +
                    "caminhoDestino=/firebird/data/VERSAODESATUALIZADA.FDB\n" +
                    "usuarioDestino=SYSDBA\n" +
                    "senhaDestino=masterkey";
                File.WriteAllText(configIpsPath, conteudo);
            }

            // Ler configIPS
            foreach (var linha in File.ReadAllLines(configIpsPath))
            {
                var partes = linha.Split('=');
                if (partes.Length != 2) continue;
                var chave = partes[0].Trim();
                var valor = partes[1].Trim();

                switch (chave)
                {
                    case "ipOrigem": ipOrigem = valor; break;
                    case "caminhoOrigem": caminhoOrigem = valor; break;
                    case "usuarioOrigem": usuarioOrigem = valor; break;
                    case "senhaOrigem": senhaOrigem = valor; break;
                    case "ipDestino": ipDestino = valor; break;
                    case "caminhoDestino": caminhoDestino = valor; break;
                    case "usuarioDestino": usuarioDestino = valor; break;
                    case "senhaDestino": senhaDestino = valor; break;
                }
            }
        }

        private void btnEnviar_Click(object sender, EventArgs e)
        {
            ExecutarBackupERestore(ipOrigem, caminhoOrigem, usuarioOrigem, senhaOrigem,
                                ipDestino, caminhoDestino, usuarioDestino, senhaDestino);
        }

        private void btnBaixar_Click(object sender, EventArgs e)
        {
            ExecutarBackupERestore(ipDestino, caminhoDestino, usuarioDestino, senhaDestino,
                                ipOrigem, caminhoOrigem, usuarioOrigem, senhaOrigem);
        }

        private void ExecutarBackupERestore(string ipOrigem, string caminhoOrigem, string usuarioOrigem, string senhaOrigem,
                                            string ipDestino, string caminhoDestino, string usuarioDestino, string senhaDestino)
        {
            string caminhoFBK = Path.Combine(pastaConfig, "backup.fbk");

            string comandoBackupArgs = $"-b -user {usuarioOrigem} -pas {senhaOrigem} {ipOrigem}:{caminhoOrigem} \"{caminhoFBK}\"";
            string comandoRestoreArgs = $"-c -user {usuarioDestino} -pas {senhaDestino} \"{caminhoFBK}\" {ipDestino}:{caminhoDestino} -replace_database";

            try
            {
                ExecutarComando(comandoBackupArgs);
                ExecutarComando(comandoRestoreArgs);
                // File.Delete(caminhoFBK); // Descomente se quiser limpar o arquivo
                MessageBox.Show("Migração concluída com sucesso.", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro durante migração: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExecutarComando(string argumentos)
        {
            var psi = new ProcessStartInfo
            {
                FileName = gbakPath, // caminho sem aspas, mesmo com espaços
                Arguments = argumentos,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var processo = Process.Start(psi);
            string output = processo.StandardOutput.ReadToEnd();
            string error = processo.StandardError.ReadToEnd();
            processo.WaitForExit();

            File.WriteAllText(Path.Combine(pastaConfig, "last_output.log"), output);
            File.WriteAllText(Path.Combine(pastaConfig, "last_error.log"), error);

            if (processo.ExitCode != 0)
                throw new Exception($"Erro ao executar:\n{gbakPath} {argumentos}\n\nSaída:\n{output}\n\nErro:\n{error}");
        }



    }
}